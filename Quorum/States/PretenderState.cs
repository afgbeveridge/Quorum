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

    public class PretenderState : BaseState<IExecutionContext>{

        //public IWriteableChannel ChannelPrototype { get; set; }

        //public IConfiguration Configuration { get; set; }

        public override async Task<StateResult> OnEntry(IStateMachineContext<IExecutionContext> context) {
            //ChannelPrototype.Respond(context.CurrentEvent.ResponseContainer, "Accepted", Configuration.Get<int>(Constants.Configuration.ResponseLimit));
            context.ExecutionContext.InEligibleForElection = false;
            LogFacade.Instance.LogInfo("Marking self as eligible for election");
            return StateResult.None;
        }

    }
}
