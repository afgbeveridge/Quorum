using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Infra;

namespace Quorum.Integration.Http {

    public class HttpReadableChannel : BaseHttpChannel, IReadableChannel {

        public HttpReadableChannel(INetworkEnvironment network) : base(network) {
        }

        public async Task<string> Read(string address, int timeoutMs) {
            string result = null;
            try {
                var request = CreateClient(address, timeoutMs);
                LogFacade.Instance.LogInfo("Contact " + request.Address);
                result = await request.Client.GetStringAsync(request.Address);
            }
            catch (Exception ex) {
                LogFacade.Instance.LogInfo("Issue: " + ex.Message);
            }
            return result;
        }

        public virtual IReadableChannel NewInstance() {
            return new HttpReadableChannel(Network);
        }
    }
}
