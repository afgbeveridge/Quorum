using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quorum.Integration {

    public interface IPayloadParser {
        TType As<TType>(object payload);
        object As(object payload, Type t);
    }

}
