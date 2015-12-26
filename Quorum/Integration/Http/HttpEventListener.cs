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
using System.Linq;

namespace Quorum.Integration.Http {

    public class HttpExposedEventListener : BaseExposedEventListener {

        public HttpExposedEventListener(IConfiguration cfg, IEventInterpreter<IExecutionContext> interpreter, IRequestValidator validator)
            : base(cfg, interpreter, validator) {
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
                AllowCORS(context);
                ValidateRequest(context.Request.Headers, context);
                using (Stream streamResponse = context.Request.InputStream) {
                    var content = new StreamReader(streamResponse).ReadToEnd();
                    ProcessRequest(content, new HttpResponseContainer { Response = context.Response, Status = status }, e => ((HttpResponseContainer)e.ResponseContainer).WriteAsync(AcceptedMessage).Wait());
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
            RequestValidator.ValidateRequestSize(ctx.Request.ContentLength64);
            // Some proxies or tools mangle custom header names, so we lower case all header names
            RequestValidator.ValidateDirectives(headers.AllKeys.ToDictionary(k => k.ToLowerInvariant(), k => headers[k]), ctx);
        }

    }

}
