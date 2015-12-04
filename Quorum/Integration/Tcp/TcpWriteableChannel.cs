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
            var splitTimeout = timeoutMs / 2;
            LogFacade.Instance.LogInfo("Base tcp timeout set at ms = " + timeoutMs + ", split for this transport: " + splitTimeout);
            var client = new TcpClient() { SendTimeout = splitTimeout, ReceiveTimeout = splitTimeout };
            string result = null;
            try {
                var res = client.BeginConnect(address, Configuration.Get<int>(Constants.Configuration.ExternalEventListenerPort), new AsyncCallback(ConnectionCallback), new CallbackState { Client = client, Address = address });
                var connectTimeout = Configuration.Get(Constants.Configuration.TcpConnectionTimeout);
                LogFacade.Instance.LogInfo("Wait for connect with " + address + ", wait period: " + connectTimeout);
                if (!await res.AsyncWaitHandle.WaitOneAsync(connectTimeout)) {
                    LogFacade.Instance.LogInfo("Giving up waiting to connect with " + address);
                    res.AsyncWaitHandle.Close();
                }
                if (!Connected)
                    LogFacade.Instance.LogInfo("Deffo not connected to " + address);
                else {
                    LogFacade.Instance.LogInfo("Tcp connection established with " + address);
                    var stream = client.GetStream();
                    TcpBoundedFrame frame = new TcpBoundedFrame();
                    int written = await frame.FrameAndWrite(stream, content);
                    LogFacade.Instance.LogInfo("Written bytes to " + address + ", count " + written);
                    result = await stream.ReadAll(TcpBoundedFrame.DetermineFrameSize);
                    LogFacade.Instance.LogInfo("Read from " + address + ": '" + result + "'");
                }
            }
            finally {
                //client.Close();
            }
            return result;
        }

        private void ConnectionCallback(IAsyncResult result) {
            CallbackState state = null;
            try {
                state = result.AsyncState as CallbackState;
                if (state != null && state.Client != null) {
                    state.Client.EndConnect(result);
                    Connected = true;
                }
            }
            catch {
                Connected = false;
                LogFacade.Instance.LogInfo("Tcp connection NOT established with " + state.Address);
            }
        }

        private bool Connected { get; set; }

        public async Task Respond(object responseChannel, string content, int timeoutMs) {
            var response = responseChannel as NetworkStream;
            if (response.IsNotNull()) {
                await new TcpBoundedFrame().FrameAndWrite(response, content).ConfigureAwait(false);
            }
        }

        public IWriteableChannel NewInstance() {
            return new TcpWriteableChannel(Configuration);
        }

        private IConfiguration Configuration { get; set; }

        private class CallbackState {
            internal TcpClient Client { get; set; }
            internal string Address { get; set; }
        }
    }

}
