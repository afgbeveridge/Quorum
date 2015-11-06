using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infra;
using System.Net;

namespace Quorum.Integration {
    
    public class SimpleNetworkEnvironment : INetworkEnvironment {

        public IPAddress LocalIPAddress {
            get {
                return Dns
                        .GetHostEntry(Dns.GetHostName())
                        .AddressList
                        .First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            }
        }

        public string HostName {
            get {
                return Dns.GetHostName();
            }
        }
    }
}
