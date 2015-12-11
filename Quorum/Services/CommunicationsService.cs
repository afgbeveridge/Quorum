﻿#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSM;
using Quorum.Integration;
using Quorum.Payloads;
using System.Configuration;
using Infra;
using System.DirectoryServices;
using System.Net;
using System.Diagnostics;

namespace Quorum.Services {

    public class CommunicationsService : ICommunicationsService {

        public IWriteableChannel ChannelPrototype { get; set; }

        public IConfiguration Configuration { get; set; }

        public IPayloadParser Parser { get; set; }

        public IPayloadBuilder Builder { get; set; }

        // Discover all neighbours, using the configured channel
        public async Task<IEnumerable<Neighbour>> DiscoverExcept(string invokingHostName) {
            var staticNeighbours = Configuration.Get<string>(Constants.Configuration.Nexus.Key);
            var neighbours = !String.IsNullOrEmpty(staticNeighbours) ? staticNeighbours.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Except(new[] { invokingHostName }, StringComparer.InvariantCultureIgnoreCase) : null;
            var result = Enumerable.Empty<Neighbour>();
            return await Query(neighbours);
        }

        public async Task<IEnumerable<Neighbour>> Query(IEnumerable<string> targets, bool includeNonResponders = false) {
            var result = Enumerable.Empty<Neighbour>();
            if (targets.IsNotNull()) {
                LogFacade.Instance.LogInfo("Apparent neighbours/targets " + String.Join(", ", targets));
                Task<Neighbour>[] queries = targets.Select(s => QueryNeighbour(s)).ToArray();
                LogFacade.Instance.LogInfo("Wait for " + queries.Count() + " task(s)");
                await Task.WhenAll(queries);
                result = queries.Where(t => !t.IsFaulted && t.Result.IsNotNull() && (t.Result.IsValid || includeNonResponders)).Select(t => t.Result);
            }
            return result;
        }

        public IEnumerable<BasicMachine> VisibleComputers(bool workgroupOnly = false) {
            Func<string, IEnumerable<DirectoryEntry>> immediateChildren = key => new DirectoryEntry("WinNT:" + key)
                    .Children
                    .Cast<DirectoryEntry>();
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

        public bool RenderEligible(string name) {
            return WriteNeighbour(name, "{\"TypeHint\": \"PretenderState\" }").IsNotNull();
        }

        public bool RenderInEligible(string name) {
            return WriteNeighbour(name, "{\"TypeHint\": \"AbdicationState\" }").IsNotNull();
        }

        // A null neighbour is returned if cannot be reached
        private async Task<Neighbour> QueryNeighbour(string name) {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            string result = await WriteNeighbour(name.ToString(), Builder.Create(new QueryRequest { Requester = "Me", Timeout = Configuration.Get<int>(Constants.Configuration.ResponseLimit) }));
            var neighbour = result.IsNull() ? Neighbour.NonRespondingNeighbour(name) : Parser.As<Neighbour>(result);
            neighbour.LastRequestElapsedTime = watch.ElapsedMilliseconds;
            return neighbour;
        }

        private async Task<string> WriteNeighbour(string name, string content) {
            string result = null;
            try {
                LogFacade.Instance.LogInfo("Write to " + name + " with content: '" + content + "'");
                // Timeout is config
                result = await ChannelPrototype
                            .NewInstance()
                            .Write(name.ToString(), content, Configuration.Get<int>(Constants.Configuration.ResponseLimit));
                LogFacade.Instance.LogInfo("Neighbour (" + name + ") queried, with result '" + (result.IsNull() ? "<null>" : result));

            }
            catch (Exception ex) {
                LogFacade.Instance.LogInfo("Neighbour (" + name + ") queried, general exception abend '" + RecurseException(ex) + "'");
            }
            return result;
        }

        private string RecurseException(Exception ex) {
            string msg = String.Empty;
            Exception e = ex;
            while (e.IsNotNull()) {
                msg = msg + e.Message + " - ";
                e = e.InnerException;
            }
            return msg;
        }
    }
}
