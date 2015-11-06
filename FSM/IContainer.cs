using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSM {
    
    public interface IContainer {
        TType Resolve<TType>(string name = null) where TType : class;
    }

}
