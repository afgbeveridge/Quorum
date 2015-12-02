using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quorum;
using Infra;
using System.Net.Sockets;
using System.IO;

namespace Quorum.Integration.Tcp {
    
    public class TcpWriteableChannel : IWriteableChannel {

        private const int ReadBufferSize = 1024;

        public TcpWriteableChannel(IConfiguration cfg) { 
            Configuration = cfg;
        }

        public async Task<string> Write(string address, string content, int timeoutMs) {
            LogFacade.Instance.LogInfo("Base tcp timeout set at ms = " + timeoutMs);
            var splitTimeout = timeoutMs / 2;
            var client = new TcpClient() { SendTimeout = splitTimeout, ReceiveTimeout = splitTimeout };//address, Configuration.Get<int>(Constants.Configuration.ExternalEventListenerPort)) { SendTimeout = splitTimeout, ReceiveTimeout = splitTimeout };
            var res = client.BeginConnect(address, Configuration.Get<int>(Constants.Configuration.ExternalEventListenerPort), new AsyncCallback(ConnectionCallback), client);
            if (!res.AsyncWaitHandle.WaitOne(1000)) {
                res.AsyncWaitHandle.Close();
            }
            string result = null;
            if (Connected) {
                var stream = client.GetStream();
                var bytes = Encoding.ASCII.GetBytes(content);
                await stream.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
                result = await stream.ReadAll();
            }
            return result;
        }

        private void ConnectionCallback(IAsyncResult result) {
            try {
                TcpClient tcpclient = result.AsyncState as TcpClient;
                if (tcpclient != null && tcpclient.Client != null) {
                    tcpclient.EndConnect(result);
                    Connected = true;
                    LogFacade.Instance.LogInfo("Tcp connection established");
                }
            }
            catch {
                Connected = false;
                LogFacade.Instance.LogInfo("Tcp connection NOT established");
            }
        }

        private bool Connected { get; set; }

        public async Task Respond(object responseChannel, string content, int timeoutMs) {
            var response = responseChannel as NetworkStream;
            if (response.IsNotNull()) {
                var payload = Encoding.ASCII.GetBytes(content);
                await response.WriteAsync(payload, 0, payload.Length).ConfigureAwait(false);
            }
        }

        public IWriteableChannel NewInstance() {
            return new TcpWriteableChannel(Configuration);
        }

        private IConfiguration Configuration { get; set; }
    }

}
