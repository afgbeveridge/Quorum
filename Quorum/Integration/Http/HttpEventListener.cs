#region License
/*
Copyright (c) 2015 Tony Beveridge

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without 
restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to 
whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE 
AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Quorum;
using Infra;
using System.Threading;
using System.IO;
using FSM;

namespace Quorum.Integration.Http {

    public class HttpEventListener : IHttpEventListener<IExecutionContext> {

        private IConfiguration Config { get; set; }

        private IEventInterpreter<IExecutionContext> Interpreter { get; set; }

        public HttpEventListener(IConfiguration cfg, IEventInterpreter<IExecutionContext> interpreter) {
            Config = cfg;
            Interpreter = interpreter;
        }

        public IStateMachine<IExecutionContext> Machine { get; set; }

        public void Initialize() {
            Listener = new HttpListener();
            var address = Config.Get<string>(Constants.Configuration.HttpListenerPort.Key);
            Listener.Prefixes.Add(new HttpAddressingScheme { Name = Dns.GetHostName(), Port = address }.Address);
            Listener.Start();
            CancellationToken = new CancellationTokenSource();
            CancellationToken token = CancellationToken.Token;
            ListenerTask = Task.Factory.StartNew(() => {
                while (!token.IsCancellationRequested)
                    ListenerImplementation();
                Listener.Stop();
            });
        }

        public void UnInitialize() {
            CancellationToken.Cancel();
        }


        private CancellationTokenSource CancellationToken { get; set; }

        private HttpListener Listener { get; set; }

        private Task ListenerTask { get; set; }

        private void ListenerImplementation() {
            var res = Listener.BeginGetContext(new AsyncCallback(ListenerCallback), Listener);
            if (!res.AsyncWaitHandle.WaitOne(400)) {
                res.AsyncWaitHandle.Close();
            }
        }

        private void ListenerCallback(IAsyncResult result) {
            HttpListener listener = (HttpListener)result.AsyncState;
            var status = 200;
            var responseMessage = String.Empty;
            HttpListenerContext context = null;
            this.GuardedExecution(() => {
                context = listener.EndGetContext(result);
                AllowCORS(context);
                using (Stream streamResponse = context.Request.InputStream) {
                    var content = new StreamReader(streamResponse).ReadToEnd();
                    var action = Interpreter.TranslateToAction(content);
                    if (action.IsNotNull() && action.EventName.IsNotNull()) {
                        // TODO: Make payload include, the content of the request, and the status and the response (context.Response)
                        EventInstance instance = new EventInstance {
                            EventName = action.EventName,
                            Payload = content,
                            ResponseContainer = context.IsNotNull() ? new HttpResponseContainer { Response = context.Response, Status = status } : null
                        };
                        LogFacade.Instance.LogInfo("Received an external event " + action.EventName + "; pass to state machine? " + (action.ExecutableStateType.IsNull() ? "Yes" : "No"));
                        if (action.ExecutableStateType.IsNull()) {
                            ((HttpResponseContainer)instance.ResponseContainer).Write("Accepted");
                            Machine.Trigger(instance);
                        }
                        else {
                            Machine.StatisticsHandler.NoteEvent(action.EventName);
                            var exec = Machine.Container.Resolve<IState<IExecutionContext>>(action.ExecutableStateType.Name);
                            if (exec.IsNotNull())
                                exec.Execute(Machine.Context, instance);
                        }
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

    }

}
