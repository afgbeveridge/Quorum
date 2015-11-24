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

namespace Quorum.Services {

    public class DiscoveryService : IDiscoveryService {

        public IWriteableChannel ChannelPrototype { get; set; }

        public IConfiguration Configuration { get; set; }

        public IPayloadParser Parser { get; set; }

        public IPayloadBuilder Builder { get; set; }

        // Discover all neighbours, using the configured channel
        public IEnumerable<Neighbour> DiscoverExcept(string invokingHostName) {
            var staticNeighbours = Configuration.Get<string>(Constants.Configuration.Nexus.Key);
            var neighbours = !String.IsNullOrEmpty(staticNeighbours) ? staticNeighbours.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Except(new[] { invokingHostName }, StringComparer.InvariantCultureIgnoreCase) : null;
            var result = Enumerable.Empty<Neighbour>();
            return Query(neighbours);
        }

        public IEnumerable<Neighbour> Query(IEnumerable<string> targets) {
            var result = Enumerable.Empty<Neighbour>();
            if (targets.IsNotNull()) {
                LogFacade.Instance.LogInfo("Apparent neighbours/targets " + String.Join(", ", targets));
                IEnumerable<Task<Neighbour>> queries = targets.Select(s => Task.Factory.StartNew<Neighbour>(QueryNeighbour, s)).ToArray();
                LogFacade.Instance.LogInfo("Wait for " + queries.Count() + " task(s)");
                Task.WaitAll(queries.ToArray());
                result = queries.Where(t => !t.IsFaulted && t.Result.IsNotNull()).Select(t => t.Result);
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

        // A null neighbour is returned if cannot be reached
        private Neighbour QueryNeighbour(object name) {
            string result = null;
            try {
                // Timeout is config
                var task = ChannelPrototype.NewInstance().Write(name.ToString(), Builder.Create(new QueryRequest { Requester = "Me" }), Configuration.Get<int>(Constants.Configuration.ResponseLimit));
                task.Wait();
                result = task.IsFaulted ? null : task.Result;
                LogFacade.Instance.LogInfo("Neighbour (" + name + ") queried, with result '" + result + "'. Faulted? " + task.IsFaulted);

            }
            catch (AggregateException ex) {
                LogFacade.Instance.LogInfo("Neighbour (" + name + ") queried, aggregated exception abend '" + RecurseException(ex) + "'");
            }
            catch (Exception ex) {
                LogFacade.Instance.LogInfo("Neighbour (" + name + ") queried, general exception abend '" + RecurseException(ex) + "'");
            }
            return result.IsNull() ? null : Parser.As<Neighbour>(result); ;
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
