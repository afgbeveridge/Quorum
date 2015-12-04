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


namespace Quorum.Integration {

    public abstract class BaseExposedEventListener : IExposedEventListener<IExecutionContext> {

        protected const string AcceptedMessage = "Accepted";

        protected IConfiguration Config { get; private set; }

        protected IEventInterpreter<IExecutionContext> Interpreter { get; private set; }

        public BaseExposedEventListener(IConfiguration cfg, IEventInterpreter<IExecutionContext> interpreter) {
            Config = cfg;
            Interpreter = interpreter;
        }

        public IStateMachine<IExecutionContext> Machine { get; set; }

        public void Initialize() {
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
                    LogFacade.Instance.LogInfo("Respond with pro-forma intent");
                    onTrigger(instance);
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

    }

}
