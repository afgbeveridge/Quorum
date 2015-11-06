using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSM {
    
    public interface IEventInterpreter<TContext> {
        InterpreterResult<TContext> TranslateToAction(object inbound);
    }

}
