#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Infra;
using System.Diagnostics;

namespace Quorum.Integration.Http {
    
    public class HttpWriteableChannel : BaseHttpChannel, IWriteableChannel {

        public HttpWriteableChannel(INetworkEnvironment network, IConfiguration cfg) : base(network, cfg) {
        }

        public async Task<string> Write(string address, string content, int timeoutMs) {
            var request = CreateClient(address, timeoutMs, ActionDisposition.Write);
            try {
                LogFacade.Instance.LogInfo("Contact (write to, timeout is " + timeoutMs + "): " + request.Address);
                var msg = CreateRequest(request.Address, HttpMethod.Post, new ByteArrayContent(Encoding.Default.GetBytes(content)));
                CancellationTokenSource cts = new CancellationTokenSource(timeoutMs);
                var response = Task.Run(() => request.Client.SendAsync(msg), cts.Token).Result;
                cts.Dispose();
                string result = null;
                LogFacade.Instance.LogDebug("Success?: " + response.IsSuccessStatusCode + ", code: " + response.StatusCode);
                if (response.IsSuccessStatusCode)
                    result = await response.Content.ReadAsStringAsync();
                LogFacade.Instance.LogDebug("Content read: " + result);
                return result;
            }
            finally {
                this.GuardedExecution(() => request.Client.Dispose());
            }
        }

        public async Task Respond(object responseChannel, string content, int timeoutMs) {
            var response = responseChannel as HttpResponseContainer;
            if (response.IsNotNull()) {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                await response.WriteAsync(content);
                watch.Stop();
                LogFacade.Instance.LogDebug("Query response written in ms = " + watch.ElapsedMilliseconds);
            }
        }

        public virtual IWriteableChannel NewInstance() {
            return new HttpWriteableChannel(Network, Config);
        }
    }
}
