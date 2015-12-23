#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
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
            var cfg = new Configuration();
            UseSsl = cfg.Get(Constants.Configuration.EncryptedTransportRequired);
            Port = cfg.Get(UseSsl ? Constants.Configuration.ExternalSecureEventListenerPort : Constants.Configuration.ExternalEventListenerPort).ToString();
        }

        public string Port { get; set; }

        private string Path { get; set; }

        public string Name { get; set; }

        public string Address { get { return new UriBuilder((UseSsl ? Uri.UriSchemeHttps: Uri.UriSchemeHttp) + "://" + Name + ":" + Port + "/" + Path).ToString(); } }

        public bool UseSsl { get; set; }
    }

}
