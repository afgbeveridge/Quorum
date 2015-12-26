#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Threading.Tasks;
using Infra;
using System.Threading;
using FSM;


namespace Quorum.Integration {

    public abstract class BaseExposedEventListener : IExposedEventListener<IExecutionContext> {

        protected const string AcceptedMessage = "Accepted";

        protected IConfiguration Config { get; private set; }

        protected IEventInterpreter<IExecutionContext> Interpreter { get; private set; }

        protected IRequestValidator RequestValidator { get; private set; }

        protected bool Secure {
            get {
                return Config.Get(Constants.Configuration.EncryptedTransportRequired);
            }
        }

        public BaseExposedEventListener(IConfiguration cfg, IEventInterpreter<IExecutionContext> interpreter, IRequestValidator validator) {
            Config = cfg;
            Interpreter = interpreter;
            RequestValidator = validator;
        }

        public IStateMachine<IExecutionContext> Machine { get; set; }

        public void Initialize() {
            LogFacade.Instance.LogInfo(ActiveDisposition.Current + " listener will be active on port " + Config.Get(Secure ? Constants.Configuration.ExternalSecureEventListenerPort : Constants.Configuration.ExternalEventListenerPort));
            StartListening();
            CancellationToken = new CancellationTokenSource();
            CancellationToken token = CancellationToken.Token;
            ListenerTask = Task.Factory.StartNew(() => {
                while (!token.IsCancellationRequested)
                    ListenerImplementation();
                StopListening();
            });
        }

        public void UnInitialize() {
            CancellationToken.Cancel();
        }

        protected abstract void StartListening();

        protected abstract void StopListening();

        private CancellationTokenSource CancellationToken { get; set; }

        private Task ListenerTask { get; set; }

        protected abstract Task ListenerImplementation();

        protected void ProcessRequest(string content, object responseContainer, Action<IEventInstance> onTrigger) {
            LogFacade.Instance.LogInfo("Received a message for interpretation: " + content);
            var action = Interpreter.TranslateToAction(content);
            if (action.IsNotNull() && action.EventName.IsNotNull()) {
                // TODO: Make payload include, the content of the request, and the status and the response (context.Response)
                EventInstance instance = new EventInstance {
                    EventName = action.EventName,
                    Payload = content,
                    ResponseContainer = responseContainer
                };
                LogFacade.Instance.LogInfo("Received an external event " + action.EventName + "; pass to state machine? " + (action.ExecutableStateType.IsNull() ? "Yes" : "No"));
                if (action.ExecutableStateType.IsNull()) {
                    LogFacade.Instance.LogDebug("Respond with pro-forma intent");
                    onTrigger(instance);
                    Machine.Trigger(instance);
                }
                else {
                    Machine.StatisticsHandler.NoteEvent(action.EventName);
                    var exec = Machine.Container.Resolve<IState<IExecutionContext>>(action.ExecutableStateType.Name);
                    // Caution as we are running an async method in a synchronous manner
                    if (exec.IsNotNull())
                        Task.Run(() => exec.Execute(Machine.Context, instance)).Wait();
                }
            }
        }

    }

}
