using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSM;

namespace Quorum.States {

    public class MasterState : BaseQuorumState {

        public override async Task<StateResult> OnEntry(IStateMachineContext<IExecutionContext> context) {
            context.ExecutionContext.IsMaster = true;
            EnsureWorkerActive(context.ExecutionContext);
            return StateResult.None;
        }

    }
}
