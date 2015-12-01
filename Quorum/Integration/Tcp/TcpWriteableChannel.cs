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
            var client = new TcpClient(address, Configuration.Get<int>(Constants.Configuration.ExternalEventListenerPort)) {  SendTimeout = timeoutMs, ReceiveTimeout = timeoutMs };
            var stream = client.GetStream();
            var bytes = Encoding.ASCII.GetBytes(content);
            await stream.WriteAsync(bytes, 0, bytes.Length);
            return await stream.ReadAll();
        }

        public async Task Respond(object responseChannel, string content, int timeoutMs) {
            var response = responseChannel as NetworkStream;
            if (response.IsNotNull()) {
                var payload = Encoding.ASCII.GetBytes(content);
                await response.WriteAsync(payload, 0, payload.Length);
            }
        }

        public IWriteableChannel NewInstance() {
            return new TcpWriteableChannel(Configuration);
        }

        private IConfiguration Configuration { get; set; }
    }

}
