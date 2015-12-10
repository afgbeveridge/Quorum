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
            try {
                if (Listener.Pending()) {
                    LogFacade.Instance.LogInfo("Processing a Tcp connection request");
                    var client = Listener.AcceptTcpClient();
                    var stream = client.GetStream();
                    var content = await stream.ReadAll(TcpBoundedFrame.DetermineFrameSize);
                    ProcessRequest(content, stream, e => ((NetworkStream)e).Write(AcceptedMessageBytes, 0, AcceptedMessageBytes.Length));
                }
            }
            catch (Exception ex) {
                LogFacade.Instance.LogWarning("Failed when processing a Tcp connection request", ex);
            }
        }

        private INetworkEnvironment NetworkHelper { get; set; }

        private TcpListener Listener { get; set; }

    }
}
