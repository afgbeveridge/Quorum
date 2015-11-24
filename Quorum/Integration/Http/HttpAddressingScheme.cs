using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Infra;

namespace Quorum.Integration.Http {
    
    public class HttpAddressingScheme : IAddressingScheme {

        private INetworkEnvironment Network { get; set; }

        public HttpAddressingScheme() {  }

        public HttpAddressingScheme(INetworkEnvironment network) {
            Network = network;
            Name = Network.LocalIPAddress.ToString();
            Port = new Infra.Configuration().Get<int>(Constants.Configuration.HttpListenerPort).ToString();
        }

        public string Port { get; set; }

        private string Path { get; set; }

        public string Name { get; set; }

        public string Address { get { return new UriBuilder(Uri.UriSchemeHttp + "://" + Name + ":" + Port + "/" + Path).ToString(); } }
    }

}
