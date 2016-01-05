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
using System.Net.Security;
using System.IO;
using Infra;
using FSM;
using Quorum.Services;
using System.Security.Cryptography.X509Certificates;

namespace Quorum.Integration.Tcp {

    public class TcpExposedEventListener : BaseExposedEventListener {

        private static readonly byte[] AcceptedMessageBytes = Encoding.ASCII.GetBytes(AcceptedMessage);

        public TcpExposedEventListener(IConfiguration cfg, IEventInterpreter<IExecutionContext> interpreter, INetworkEnvironment env, ISecurityService svc) : base(cfg, interpreter, svc) {
            NetworkHelper = env;
        }

        protected override void StartListening() {
            Listener = new TcpListener(NetworkHelper.LocalIPAddress, Config.Get(Secure ? Constants.Configuration.ExternalSecureEventListenerPort : Constants.Configuration.ExternalEventListenerPort));
            Listener.Start(Config.Get(Constants.Configuration.TcpBacklogSize));
        }

        protected override void StopListening() {
            Listener.Stop();
        }

        protected override async Task ListenerImplementation() {
            try {
                if (Listener.Pending()) {
                    LogFacade.Instance.LogInfo("Processing a Tcp connection request");
                    var client = Listener.AcceptTcpClient();
                    var stream = OpenStream(client);
                    int? size = await TcpBoundedFrame.DetermineFrameSize(stream);
                    if (size.HasValue)
                        SecurityService.ValidateRequestSize(size.Value);
                    var result = TcpBoundedFrame.Parse(await stream.ReadAll(size));
                    // Check directives
                    ValidateRequest(result.Directives, client);
                    ProcessRequest(result.Content, stream, e => ((Stream)e.ResponseContainer).Write(AcceptedMessageBytes, 0, AcceptedMessageBytes.Length));
                }
            }
            catch (Exception ex) {
                LogFacade.Instance.LogWarning("Failed when processing a Tcp connection request", ex);
            }
        }

        private Stream OpenStream(TcpClient client) {
            Stream stream = client.GetStream();
            var secure = Config.Get(Constants.Configuration.EncryptedTransportRequired);
            if (secure) {
                SslStream enc = new SslStream(stream, true);
                // As we are looking at the common name, we mark up the host name
                var hostName = "cn=" + NetworkHelper.HostName.ToLowerInvariant();
                var cert = Crypto.GetCertificate(StoreName.My, StoreLocation.LocalMachine, c => c.Subject.ToLowerInvariant().Trim() == hostName);
                enc.AuthenticateAsServer(cert, false, System.Security.Authentication.SslProtocols.Default, false);
                stream = enc;
            }
            return stream;
        }

        private void ValidateRequest(IDictionary<string, string> directives, TcpClient client) {
            SecurityService.ValidateDirectives(directives, client);
        }

        private INetworkEnvironment NetworkHelper { get; set; }

        private TcpListener Listener { get; set; }

    }
}
