using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSM;

namespace Quorum.States {

    public class QuiescentState : BaseQuorumState {

        public override async Task<StateResult> OnEntry(IStateMachineContext<IExecutionContext> context) {
            await DeActivateWorker(context.ExecutionContext);
            context.ExecutionContext.IsMaster = false;
            return StateResult.None;
        }

    }
    
}
