#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System.Linq;
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
