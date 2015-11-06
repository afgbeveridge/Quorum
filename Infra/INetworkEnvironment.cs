using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Infra {
    
    public interface INetworkEnvironment {

        IPAddress LocalIPAddress { get; }

        string HostName { get; }

    }

}
