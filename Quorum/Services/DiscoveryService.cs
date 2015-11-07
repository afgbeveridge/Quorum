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

namespace Quorum.Services {
    
    public class DiscoveryService : IDiscoveryService {

        public IWriteableChannel ChannelPrototype { get; set; }

        public IConfiguration Configuration { get; set; }

        public IPayloadParser Parser { get; set; }

        public IPayloadBuilder Builder { get; set; }

        // Discover all neighbours, using the configured channel
        public IEnumerable<Neighbour> Discover(string invokingHostName) {
            var staticNeighbours = Configuration.Get<string>(Constants.Configuration.Nexus.Key);
            var neighbours = !String.IsNullOrEmpty(staticNeighbours) ? staticNeighbours.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Except(new[] { invokingHostName }) : null;
            var result = Enumerable.Empty<Neighbour>();
            if (neighbours.IsNotNull()) {
                LogFacade.Log("Apparent neighbours " + String.Join(", ", neighbours));
                IEnumerable<Task<Neighbour>> queries = neighbours.Select(s => Task.Factory.StartNew<Neighbour>(QueryNeighbour, s)).ToArray();
                LogFacade.Log("Wait for " + queries.Count() + " task(s)");
                Task.WaitAll(queries.ToArray());
                result = queries.Where(t => !t.IsFaulted && t.Result.IsNotNull()).Select(t => t.Result);
            }
            return result;
        }

        // A null neighbour is returned if cannot be reached
        private Neighbour QueryNeighbour(object name) {
            string result = null;
            try {
                // Timeout is config
                var task = ChannelPrototype.NewInstance().Write(name.ToString(), Builder.Create(new QueryRequest { Requester = "Me" }), Configuration.Get<int>(Constants.Configuration.ResponseLimit));
                task.Wait();
                result = task.IsFaulted ? null : task.Result;
                LogFacade.Log("Neighbour (" + name + ") queried, with result '" + result + "'");

            }
            catch (AggregateException ex) {
                LogFacade.Log("Neighbour (" + name + ") queried, abend '" + ex.InnerException.Message + "'");
            }
            catch (Exception ex) {
                LogFacade.Log("Neighbour (" + name + ") queried, abend '" + ex.Message + "'");
            }
            return result.IsNull() ? null : Parser.As<Neighbour>(result); ;
        }
    }
}
