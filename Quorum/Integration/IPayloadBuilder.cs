using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quorum.Integration {
    
    public interface IPayloadBuilder {

        string Create<TType>(TType obj);

    }
}
