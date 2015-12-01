using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Infra;
using System.Diagnostics;

namespace Quorum.Integration.Http {
    
    public class HttpWriteableChannel : BaseHttpChannel, IWriteableChannel {

        public HttpWriteableChannel(INetworkEnvironment network) : base(network) {
        }

        public async Task<string> Write(string address, string content, int timeoutMs) {
            var request = CreateClient(address, timeoutMs, ActionDisposition.Write);
            LogFacade.Instance.LogInfo("Contact (write to): " + request.Address);
            var response = await request.Client.PostAsync(request.Address, new ByteArrayContent(Encoding.Default.GetBytes(content))).ConfigureAwait(false);
            string result = null;
            LogFacade.Instance.LogInfo("Success?: " + response.IsSuccessStatusCode);
            if (response.IsSuccessStatusCode) 
                result = await response.Content.ReadAsStringAsync();
            LogFacade.Instance.LogInfo("Content read: " + result);
            return result;
        }

        public async Task Respond(object responseChannel, string content, int timeoutMs) {
            var response = responseChannel as HttpResponseContainer;
            if (response.IsNotNull()) {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                await response.WriteAsync(content);
                watch.Stop();
                LogFacade.Instance.LogInfo("Query response written in ms = " + watch.ElapsedMilliseconds);
            }
        }

        public virtual IWriteableChannel NewInstance() {
            return new HttpWriteableChannel(Network);
        }
    }
}
