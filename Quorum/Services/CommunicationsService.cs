#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quorum.Integration;
using Quorum.Payloads;
using Infra;
using System.DirectoryServices;
using System.Net;
using System.Net.NetworkInformation;
using System.Diagnostics;
using FSM;

namespace Quorum.Services {

    public class CommunicationsService : ICommunicationsService {

        public CommunicationsService(INetworkEnvironment env) {
            Network = env;
        }

        private INetworkEnvironment Network { get; set; }

        public IWriteableChannel ChannelPrototype { get; set; }

        public IConfiguration Configuration { get; set; }

        public IPayloadParser Parser { get; set; }

        public IPayloadBuilder Builder { get; set; }

        // Discover all neighbours, using the configured channel
        public async Task<IEnumerable<Neighbour>> DiscoverExcept(string invokingHostName, bool includeNonResponders = false) {
            var staticNeighbours = Configuration.Get<string>(Constants.Configuration.Nexus.Key);
            var neighbours = !String.IsNullOrEmpty(staticNeighbours) ? staticNeighbours.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Except(new[] { invokingHostName }, StringComparer.InvariantCultureIgnoreCase) : null;
            var result = Enumerable.Empty<Neighbour>();
            return await Query(neighbours, includeNonResponders);
        }

        public async Task<IEnumerable<Neighbour>> Query(IEnumerable<string> targets, bool includeNonResponders = false) {
            var result = Enumerable.Empty<Neighbour>();
            if (targets.IsNotNull()) {
                LogFacade.Instance.LogInfo("Apparent neighbours/targets " + String.Join(", ", targets));
                Task<Neighbour>[] queries = targets.Select(s => QueryNeighbour(s)).ToArray();
                LogFacade.Instance.LogDebug("Wait for " + queries.Count() + " task(s)");
                await Task.WhenAll(queries);
                result = queries.Where(t => !t.IsFaulted && t.Result.IsNotNull() && (t.Result.IsValid || includeNonResponders)).Select(t => t.Result);
            }
            return result;
        }

        public async Task<BroadcastDiscoveryResult> BroadcastDiscovery(IEnumerable<string> targets, IEnumerable<string> possibleNeighbours) {
            var request = Builder.Create(new OutOfBandDiscoveryRequest { MootedNeighbours = possibleNeighbours });
            Task<string>[] queries = targets.Select(s => WriteNeighbour(s, request)).ToArray();
            LogFacade.Instance.LogDebug("Wait for " + queries.Count() + " task(s)");
            await Task.WhenAll(queries);
            return new BroadcastDiscoveryResult {
                QueriedMembers = queries
                                    .Where(t => !t.IsFaulted && t.Result.IsNotNull())
                                    .Select(t => Parser.As<DiscoveryEncapsulate>(t.Result))
                                    .ToArray()
            };
        }

        public IEnumerable<BasicMachine> VisibleComputers(bool workgroupOnly = false) {
            Func<string, IEnumerable<DirectoryEntry>> immediateChildren = key => 
                    new DirectoryEntry("WinNT:" + key).Children.Cast<DirectoryEntry>();
            Func<IEnumerable<DirectoryEntry>, IEnumerable<BasicMachine>> qualifyAndSelect = entries => entries.Where(c => c.SchemaClassName == "Computer")
                    .Select(c => { 
                        var addresses = Dns.GetHostAddresses(c.Name);
                        var addr = addresses.IsNull() || !addresses.Any() ? "Unknown" : addresses.First(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
                        return new BasicMachine { Name = c.Name, IpAddressV4 = addr }; 
                    });
            return (
                !workgroupOnly ?
                    qualifyAndSelect(immediateChildren(String.Empty)
                        .SelectMany(d => d.Children.Cast<DirectoryEntry>())) 
                    :
                    qualifyAndSelect(immediateChildren("//WORKGROUP"))
            ).ToArray();
        }

        public async Task<IEnumerable<BasicMachine>> Ping(IEnumerable<string> targets, int timeoutMs) {
            var result = Enumerable.Empty<BasicMachine>();
            if (targets.IsNotNull()) {
                LogFacade.Instance.LogInfo("(Ping) Apparent neighbours/targets " + string.Join(", ", targets) + ", patient for ms = " + timeoutMs);
                Task<BasicMachine>[] queries = targets.Select(s => GuardedPing(s, timeoutMs)).ToArray();
                LogFacade.Instance.LogDebug("Wait for " + queries.Count() + " task(s)");
                await Task.WhenAll(queries);
                result = queries.Where(t => !t.IsFaulted && t.Result.IsNotNull() && t.Result.Name.IsNotNull()).Select(t => t.Result).ToArray();
            }
            return result;
        }

        private async Task<BasicMachine> GuardedPing(string name, int timeoutMs) {
            BasicMachine mc = null;
            try {
                var reply = await new Ping().SendPingAsync(name, timeoutMs);
                if (reply.Status == IPStatus.Success)
                    mc = new BasicMachine {
                        Name = name,
                        IpAddressV4 = reply.Address.ToString()
                    };
            }
            catch {
            }
            return mc;
        }

        public bool RenderEligible(string name) {
            return WriteNeighbour(name, "{\"TypeHint\": \"PretenderState\" }").IsNotNull();
        }

        public bool RenderInEligible(string name) {
            return WriteNeighbour(name, "{\"TypeHint\": \"AbdicationState\" }").IsNotNull();
        }

        public async Task<AnalysisResult> Analyze(IContainer container, string name) {
            Neighbour result = null;
            var names = Enum.GetNames(typeof(TransportType));
            string protocol = null;
            // Rather old school use of a for loop as c# gets all confused using an async lambda with a FirstOrDefault(), which was my preferred approach
            for (int i = 0; i < names.Length && (result.IsNull() || !result.IsValid); i++) {
                protocol = names[i];
                ActiveDisposition.AcceptTransportType(protocol);
                result = await QueryNeighbour(name, container.Resolve<IWriteableChannel>());
            }
            return new AnalysisResult { Protocol = result.IsNull() || !result.IsValid ? null : protocol.ToString() };
        }

        public Task OfferConfiguration(IEnumerable<string> targets, IEnumerable<string> quorumMembers) {
            targets
                .ToList()
                .ForEach(async t => {
                    await WriteNeighbour(t, Builder.Create(new ConfigurationBroadcast { QuorumMembers = quorumMembers.ToArray() }));
                });
            return Task.FromResult(0);
        }


        // A null neighbour is returned if cannot be reached
        public async Task<Neighbour> QueryNeighbour(string name, IWriteableChannel channel = null) {
            Neighbour n = null;
            try {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                string result = await WriteNeighbour(name.ToString(), 
                                              Builder.Create(new QueryRequest { Requester = Network.HostName, Timeout = Configuration.Get(Constants.Configuration.ResponseLimit) }),
                                              channel);
                n = result.IsNull() ? Neighbour.NonRespondingNeighbour(name) : Parser.As<Neighbour>(result);
                watch.Stop();
                n.LastRequestElapsedTime = watch.ElapsedMilliseconds;
            }
            catch (Exception ex) {
                LogFacade.Instance.LogException("Neighbour write and parse failed for " + name, ex);
                n = Neighbour.NonRespondingNeighbour(name);
            }
            LogFacade.Instance.LogDebug("QueryNeighbour " + name + " completed in " + n.LastRequestElapsedTime + " ms");
            return n;
        }

        private async Task<string> WriteNeighbour(string name, string content, IWriteableChannel channel = null) {
            string result = null;
            try {
                LogFacade.Instance.LogDebug("Write to " + name + " with content: '" + content + "'");
                var chan = channel ?? ChannelPrototype.NewInstance();
                // Timeout is config
                result = await chan.Write(name.ToString(), content, Configuration.Get(Constants.Configuration.ResponseLimit));
                LogFacade.Instance.LogDebug("Neighbour (" + name + ") queried, with result '" + (result.IsNull() ? "<null>" : result));

            }
            catch (Exception ex) {
                LogFacade.Instance.LogWarning("Neighbour (" + name + ") queried, general exception abend '" + RecurseException(ex) + "'");
            }
            return result;
        }

        private string RecurseException(Exception ex) {
            string msg = string.Empty;
            Exception e = ex;
            while (e.IsNotNull()) {
                msg = msg + e.Message + " - ";
                e = e.InnerException;
            }
            return msg;
        }
    }
}
