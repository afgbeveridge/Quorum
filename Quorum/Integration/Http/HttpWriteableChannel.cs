using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Infra;

namespace Quorum.Integration.Http {
    
    public class HttpWriteableChannel : BaseHttpChannel, IWriteableChannel {

        public HttpWriteableChannel(INetworkEnvironment network) : base(network) {
        }

        public async Task<string> Write(string address, string content, int timeoutMs) {
            var request = CreateClient(address, timeoutMs, ActionDisposition.Write);
            LogFacade.Instance.LogInfo("Contact: " + request.Address);
            var response = await request.Client.PostAsync(request.Address, new ByteArrayContent(Encoding.Default.GetBytes(content)));
            string result = null;
            if (response.IsSuccessStatusCode) 
                result = await response.Content.ReadAsStringAsync();
            return result;
        }


        public void Respond(object responseChannel, string content, int timeoutMs) {
            var response = responseChannel as HttpResponseContainer;
            if (response.IsNotNull())
                response.Write(content);
        }

        public virtual IWriteableChannel NewInstance() {
            return new HttpWriteableChannel(Network);
        }
    }
}
