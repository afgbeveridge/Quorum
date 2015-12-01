using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quorum.Integration;
using System.Net.Sockets;
using System.IO;
using Infra;
using Quorum;
using FSM;

namespace Quorum.Integration.Tcp {
    
    public class TcpExposedEventListener : BaseExposedEventListener {

        private static readonly byte[] AcceptedMessageBytes = Encoding.ASCII.GetBytes(AcceptedMessage);

        public TcpExposedEventListener(IConfiguration cfg, IEventInterpreter<IExecutionContext> interpreter, INetworkEnvironment env) : base(cfg, interpreter) {
            NetworkHelper = env;
        }

        protected override void StartListening() {
            Listener = new TcpListener(NetworkHelper.LocalIPAddress, Config.Get<int>(Constants.Configuration.ExternalEventListenerPort));
            Listener.Start(Config.Get<int>(Constants.Configuration.TcpBacklogSize));
        }

        protected override void StopListening() {
            Listener.Stop();
        }

        protected override async Task ListenerImplementation() {
            if (Listener.Pending()) {
                var client = Listener.AcceptTcpClient();
                var stream = client.GetStream();
                var content = await stream.ReadAll();
                ProcessRequest(content, stream, e => ((NetworkStream) e).Write(AcceptedMessageBytes, 0, AcceptedMessageBytes.Length));
                stream.Dispose();
            }
        }

        private INetworkEnvironment NetworkHelper { get; set; }

        private TcpListener Listener { get; set; }

    }
}
