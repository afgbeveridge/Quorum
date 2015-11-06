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

namespace Quorum.States {
    
    public class DiscoveryState : BaseState<IExecutionContext> {

        public IWriteableChannel ChannelPrototype { get; set; }

        public IConfiguration Configuration { get; set; }

        public IPayloadParser Parser { get; set; }

        public IPayloadBuilder Builder { get; set; }

        public override async Task<StateResult> OnEntry(IStateMachineContext<IExecutionContext> context) {
            bool reversionPossible = AllowInterrupt;
            StateResult result = StateResult.None;
            try {
                Interruptable = AllowInterrupt;
                var neighbours = ContactNeighbours(context.ExecutionContext.HostName);
                context.ExecutionContext.Network.Neighbours.Clear();
                context.ExecutionContext.Network.Neighbours.AddRange(neighbours);
                if (!neighbours.Any()) {
                    reversionPossible = false;
                    result.NextState = EventNames.Elected;
                }
                // No masters or potential split brain
                else if (!neighbours.Any(n => n.IsMaster) || context.ExecutionContext.IsMaster) { // This needs to be non concrete i.e. no reference to EventInstance
                    result.NextState = EventNames.RequestElection;
                    // Cannot revert from this state
                    reversionPossible = false;
                }
                else if (neighbours.Any(n => n.IsMaster))
                    result.NextState = EventNames.Quiescent;
            }
            finally {
                Interruptable = true;
                AllowInterrupt = true;
                context.ExecutionContext.Network.LastChecked = DateTime.Now;
            }
            result.Revert = reversionPossible && context.EnclosingMachine.HasPreviousState;
            return result;
        }

        // Discover all neighbours, using the configured channel
        private IEnumerable<Neighbour> ContactNeighbours(string hostName) {
            var staticNeighbours = Configuration.Get<string>(Constants.Configuration.Nexus.Key);
            var neighbours = !String.IsNullOrEmpty(staticNeighbours) ? staticNeighbours.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Except(new [] { hostName }) : null;
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
                LogFacade.Log("Neighbour (" + name + ") queried, abend '" + ex.Message  + "'");
            }
            return result.IsNull() ? null : Parser.As<Neighbour>(result); ;
        }

        private bool AllowInterrupt { get; set; }

    }

}
