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
    
    public class AbdicationState : DeActivatingState {

        protected override FSM.StateResult FinalResult {
            get {
                return StateResult.Create(nextState: EventNames.Quiescent);
            }
        }

        protected override void PostObit(IStateMachineContext<IExecutionContext> context) {
            //ChannelPrototype.Respond(context.CurrentEvent.ResponseContainer, "Accepted", Configuration.Get<int>(Constants.Configuration.ResponseLimit));
            context.ExecutionContext.InEligibleForElection = true;
            LogFacade.Instance.LogInfo("Marking self as IN-eligible for election");
        }

    }
}
