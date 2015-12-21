#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Infra;
using FSM;

namespace Quorum.Integration.Tcp {

    public class TcpExposedEventListener : BaseExposedEventListener {

        private static readonly byte[] AcceptedMessageBytes = Encoding.ASCII.GetBytes(AcceptedMessage);

        public TcpExposedEventListener(IConfiguration cfg, IEventInterpreter<IExecutionContext> interpreter, INetworkEnvironment env, IRequestValidator validator) : base(cfg, interpreter, validator) {
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
                    int? size = await TcpBoundedFrame.DetermineFrameSize(stream);
                    if (size.HasValue)
                        RequestValidator.ValidateRequestSize(size.Value);
                    var result = TcpBoundedFrame.Parse(await stream.ReadAll(size));
                    // Check directives
                    ValidateRequest(result.Directives, client);
                    ProcessRequest(result.Content, stream, e => ((NetworkStream)e.ResponseContainer).Write(AcceptedMessageBytes, 0, AcceptedMessageBytes.Length));
                }
            }
            catch (Exception ex) {
                LogFacade.Instance.LogWarning("Failed when processing a Tcp connection request", ex);
            }
        }

        private void ValidateRequest(IDictionary<string, string> directives, TcpClient client) {
            RequestValidator.ValidateDirectives(directives, client);
        }

        private INetworkEnvironment NetworkHelper { get; set; }

        private TcpListener Listener { get; set; }

    }
}
