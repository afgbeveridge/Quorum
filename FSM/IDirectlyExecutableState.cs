using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSM {
    
    public interface IDirectlyExecutableState<TContext> {

        StateResult Execute(IStateMachineContext<TContext> context, IEventInstance anEvent);

    }
}
