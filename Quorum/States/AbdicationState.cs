#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using FSM;
using Infra;

namespace Quorum.States {

    public class AbdicationState : DeActivatingState {

        protected override StateResult FinalResult {
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
