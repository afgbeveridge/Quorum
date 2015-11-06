using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSM {

    public interface IStateMachineContext<TContext> {
        TContext ExecutionContext { get; }
        IStateMachineChannel<TContext> EnclosingMachine { get; }
        IEventInstance CurrentEvent { get; set; }
    }
}
