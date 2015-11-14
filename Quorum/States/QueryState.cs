using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSM;
using Quorum.Integration;
using Quorum.Payloads;
using System.Net;
using Infra;

namespace Quorum.States {

    public class QueryState : BaseState<IExecutionContext> {

        public IPayloadBuilder Builder { get; set; }

        public IPayloadParser Parser { get; set; }

        public IWriteableChannel Channel { get; set; }

        public INetworkEnvironment Network { get; set; }

        public IConfiguration Configuration { get; set; }

        public override async Task<StateResult> OnEntry(IStateMachineContext<IExecutionContext> context) {
            return Execute(context, context.CurrentEvent);
        }

        public override StateResult Execute(IStateMachineContext<IExecutionContext> context, IEventInstance anEvent) {
            try {
                Interruptable = false;
                var queryResponse = Neighbour.Fabricate(context.ExecutionContext.IsMaster, context.ExecutionContext.HostName, context.EnclosingMachine.UpTime, context.ExecutionContext.InEligibleForElection);
                /// Tell the world who we are
                // context.ExecutionContext.ResultStore[context.CurrentEvent.Id] = Builder.Create(queryResponse);
                var request = Parser.As<QueryRequest>(anEvent.Payload);
                Channel.Respond(anEvent.ResponseContainer, Builder.Create(queryResponse), Configuration.Get<int>(Constants.Configuration.ResponseLimit));
            }
            catch { }
            // This is a bounce state, so we revert to the previous state
            return new StateResult { Revert = true };
        }

    }

}
