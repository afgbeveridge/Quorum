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
using System.Net.Http;
using Infra;

namespace Quorum.Integration.Http {
    
    public class BaseHttpChannel {

        public BaseHttpChannel(INetworkEnvironment network) {
            Network = network;
        }

        protected INetworkEnvironment Network { get; private set; }

        protected ClientDetails CreateClient(string address, int timeoutMs, ActionDisposition disposition = ActionDisposition.Read) {
            LogFacade.Instance.LogInfo("Base http timeout set at ms = " + timeoutMs);
            var details = new ClientDetails { 
                Client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeoutMs) }, 
                Address = new HttpAddressingScheme(Network) { Name = address }.Address
            };
            if (Modifier.IsNotNull()) {
                if (disposition == ActionDisposition.Read)
                    Modifier.PreRead(details);
                else
                    Modifier.PreWrite(details);
            }
            return details;
        }

        public IHttpClientModifier Modifier { get; set; }
    }

}
