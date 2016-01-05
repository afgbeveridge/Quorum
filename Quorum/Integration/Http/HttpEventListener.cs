#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion

using System;
using System.Threading.Tasks;
using System.Net;
using Infra;
using System.IO;
using FSM;
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using Quorum.Services;

namespace Quorum.Integration.Http {

    public class HttpExposedEventListener : BaseExposedEventListener {

        public IPayloadBuilder Builder { get; set; }

        public HttpExposedEventListener(IConfiguration cfg, IEventInterpreter<IExecutionContext> interpreter, ISecurityService svc)
            : base(cfg, interpreter, svc) {
            WaitHandleTimeout = cfg.Get(Constants.Configuration.HttpListenerWaitHandleTimeout);
        }

        protected override void StartListening() {
            Listener = new HttpListener();
            var address = Config.Get(Secure ? Constants.Configuration.ExternalSecureEventListenerPort : Constants.Configuration.ExternalEventListenerPort);
            Listener.Prefixes.Add(new HttpAddressingScheme {
                Name = Dns.GetHostName(),
                Port = address.ToString(),
                UseSsl = Secure
            }.Address);
            LogFacade.Instance.LogInfo("Http listener prefix: " + Listener.Prefixes.First());
            Listener.Start();
        }

        protected override void StopListening() {
            Listener.Stop();
        }

        private HttpListener Listener { get; set; }

        protected override Task ListenerImplementation() {
            var res = Listener.BeginGetContext(new AsyncCallback(ListenerCallback), Listener);
            if (!res.AsyncWaitHandle.WaitOne(WaitHandleTimeout)) {
                res.AsyncWaitHandle.Close();
            }
            return Task.FromResult(0);
        }

        private int WaitHandleTimeout { get; set; }

        private void ListenerCallback(IAsyncResult result) {
            HttpListener listener = (HttpListener)result.AsyncState;
            var status = 200;
            var responseMessage = String.Empty;
            HttpListenerContext context = null;
            this.GuardedExecution(() => {
                context = listener.EndGetContext(result);
                if (context.Request.IsWebSocketRequest)
                    HandleWebSocketRequest(context);
                else {
                    AllowCORS(context);
                    ValidateRequest(context.Request.Headers, context);
                    using (Stream streamResponse = context.Request.InputStream) {
                        var content = new StreamReader(streamResponse).ReadToEnd();
                        ProcessRequest(content, new HttpResponseContainer { Response = context.Response, Status = status }, e => ((HttpResponseContainer)e.ResponseContainer).WriteAsync(AcceptedMessage).Wait());
                    }
                }
            },
            ex => status = 500);
        }

        private void AllowCORS(HttpListenerContext context) {
            if (context.Request.HttpMethod == "OPTIONS") {
                context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
                context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
                context.Response.AddHeader("Access-Control-Max-Age", "1728000");
            }
            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
        }

        private void ValidateRequest(NameValueCollection headers, HttpListenerContext ctx) {
            SecurityService.ValidateRequestSize(ctx.Request.ContentLength64);
            // Some proxies or tools mangle custom header names, so we lower case all header names
            SecurityService.ValidateDirectives(headers.AllKeys.ToDictionary(k => k.ToLowerInvariant(), k => headers[k]), ctx);
        }

        private async Task HandleWebSocketRequest(HttpListenerContext ctx) {
            var socket = await Handshake(ctx);
            if (socket.IsNotNull())
                Task.Run(async () => await HandleSocket(socket), CancellationToken.Token);
        }

        private async Task<WebSocket> Handshake(HttpListenerContext ctx) {
            WebSocketContext webSocketContext = null;
            try {
                // When calling `AcceptWebSocketAsync` the negotiated subprotocol must be specified. This sample assumes that no subprotocol 
                // was requested. 
                webSocketContext = await ctx.AcceptWebSocketAsync(subProtocol: null);
                LogFacade.Instance.LogInfo("Accepted web socket connection");
            }
            catch (Exception e) {
                // The upgrade process failed somehow. For simplicity lets assume it was a failure on the part of the server and indicate this using 500.
                ctx.Response.StatusCode = 500;
                ctx.Response.Close();
            }
            return webSocketContext.IsNull() ? null : webSocketContext.WebSocket;
        }

        private async Task HandleSocket(WebSocket socket) {
            Guid key = Guid.NewGuid();
            BlockingCollection<object> collection = new BlockingCollection<object>();
            LogFacade.Instance.AddListener(key, entry => collection.Add(entry));
            try {
                bool took = true;
                await CheckHeaders(socket);
                while (socket.State == WebSocketState.Open && !CancellationToken.Token.IsCancellationRequested) {
                    if (!took) {
                        await Task.Delay(1000);
                        await WriteSocket(socket, new { Ping = true });
                    }
                    object item;
                    took = collection.TryTake(out item);
                    if (took)
                        await WriteSocket(socket, item);
                }
            }
            catch (Exception ex) {
                LogFacade.Instance.LogException("Exception handling web socket connection", ex);
            }
            finally {
                this.GuardedExecution(() => {
                    LogFacade.Instance.RemoveListener(key);
                    socket.Dispose();
                    LogFacade.Instance.LogInfo("Closed web socket connection");
                });
            }
        }

        private async Task WriteSocket(WebSocket socket, object item) {
            var outputBuffer = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(Builder.Create(item)));
            await socket.SendAsync(outputBuffer, WebSocketMessageType.Text, true, CancellationToken.Token);
        }

        // Web sockets start off as minimal HTTP connections, upgrade, and then behave like TCP sockets for the most part.
        // Thus the 'security service' abstraction is used to insulate this listener from direct TCP objects dependence.
        // Somewhat of a cheat, but it is the way it is :-)!
        private async Task CheckHeaders(WebSocket socket) {
            byte[] buffer = new byte[Config.Get(Constants.Configuration.MaxPayloadLength)];
            var response = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.Token);
            // TODO: Ensure complete message is received
            var result = Encoding.ASCII.GetString(buffer, 0, response.Count);
            LogFacade.Instance.LogDebug("Received headers for web socket verification: " + result);
            SecurityService.DissectAndValidateFrame(result);
        }

    }

}
