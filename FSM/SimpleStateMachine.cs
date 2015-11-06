using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Infra;

namespace FSM {

    public class SimpleStateMachine<TContext> : IStateMachine<TContext> {

        private List<StateDefinition> States = new List<StateDefinition>();
        private ConcurrentQueue<IEventInstance> QueuedEvents = new ConcurrentQueue<IEventInstance>();
        private SemaphoreSlim StateChangeAtomiser;

        public SimpleStateMachine() {
            CreationDateTime = DateTime.Now;
            // TODO: Config
            StateChangeAtomiser = new SemaphoreSlim(1, 1);
            // TODO: Configuration
            QueueTimer = new System.Timers.Timer(50);
            QueueTimer.Elapsed += (src, args) => { CheckQueuedEvents(); };
            QueueTimer.AutoReset = true;
            QueueTimer.Enabled = true;
        }

        private static System.Timers.Timer QueueTimer { get; set; }

        public IStateMachine<TContext> DefineState<TState>() where TState : IState<TContext> {
            Complete();
            return this.Fluently(_ => CurrentStateDefinition = new StateDefinition(Container) { StateType = typeof(TState) });
        }

        public IStateMachine<TContext> AsStartState() {
            Stable();
            return this.Fluently(_ => CurrentStateDefinition.IsStartState = true);
        }

        public IStateMachine<TContext> Singleton() {
            Stable();
            return this.Fluently(_ => CurrentStateDefinition.IsSingleton = true);
        }

        public IStateMachine<TContext> On(string eventName) {
            Stable();
            // Assert event name is null
            return this.Fluently(_ => CurrentEventName = eventName);
        }

        public IStateMachine<TContext> TransitionTo<TState>() where TState : IState<TContext> {
            Stable();
            // Assert event name is not null
            return this.Fluently(_ => {
                CurrentStateDefinition.HandledEvents[CurrentEventName] = typeof(TState);
                CurrentEventName = null;
            });
        }

        public IStateMachine<TContext> MarkAsBounceState() {
            return this.Fluently(_ => {
                CurrentStateDefinition.IsBounceState = true;
            });
        }

        public async Task<IStateMachine<TContext>> Start() {
            Complete();
            Assert.True(States.Any(), () => "This state machine is empty");
            Assert.True(States.Any(s => s.IsStartState), () => "There is no start state defined");
            return this.Fluently(async _ => {
                ActiveState = States.First(s => s.IsStartState);
                await HandleStateResult(await ActiveState.State.OnEntry(Context));
            });
        }

        public IStateMachine<TContext> Stop() {
            return this.Fluently(_ => { });
        }

        public async Task Trigger(IEventInstance anEvent) {
            // Some states may mark themselves as non interruptable, requiring self to queue events. If an event comes in, and queued events exist, maintain order
            LogFacade.Log("Trigger " + anEvent.EventName + ", resource level: " + StateChangeAtomiser.CurrentCount);
            await StateChangeAtomiser.WaitAsync();
            try {
                anEvent.PreviousStateName = HasPreviousState ? PreviousState.Name : null;
                LogFacade.Log("Proceed with processing of " + anEvent.EventName);
                if (!ActiveState.State.Interruptable || QueuedEvents.Any()) { 
                    if (!anEvent.NoQueue)
                        EnqueueEvent(anEvent);
                }
                else {
                    var type = ActiveState.EventReceived(anEvent.EventName, IgnoreUnknownEvents);
                    if (type.IsNotNull())
                        await DispatchEvent(anEvent, type);
                }
            }
            finally {
                LogFacade.Log("Release for Trigger " + anEvent.EventName);
                StateChangeAtomiser.Release();
            }
        }

        public double UpTime { 
            get {
                return (DateTime.Now - CreationDateTime).TotalMilliseconds;
            }
        }

        private DateTime CreationDateTime { get; set; }

        private async Task DispatchEvent(IEventInstance anEvent, Type type) {
            LogFacade.Log("Dispatch " + anEvent.EventName);
            Context.CurrentEvent = anEvent;
            var isReflexive = type == ActiveState.State.GetType();
            var target = States.First(s => s.StateType == type);
            LogFacade.Log("Event " + anEvent.EventName + " transitions to " + target.StateType.Name);
            if (type == ActiveState.StateType) {
                if (isReflexive)
                    await ActiveState.State.OnReflexiveEntry(Context);
                else
                    await HandleStateResult(await ActiveState.State.OnEntry(Context));
            }
            else {
                await HandleStateResult(await ActiveState.State.OnExit(Context));
                if (!ActiveState.IsBounceState) {
                    LogFacade.Log("Revertable state detected, retain: " + ActiveState.StateType.Name);
                    PreviousState = ActiveState;
                }
                ActiveState = target;
                await HandleStateResult(await ActiveState.State.OnEntry(Context));
            }
            // At the moment, consider this safe, as number of queued events likely to be low
            //await CheckQueuedEvents();
        }

        private async Task CheckQueuedEvents() {
            if (ActiveState.State.Interruptable && QueuedEvents.Any()) {
                IEventInstance anEvent;
                LogFacade.Log("Trying to dequeue an event");
                if (QueuedEvents.TryDequeue(out anEvent))
                    await Trigger(anEvent);
            }
        }

        public async Task RevertToPreviousState() {
            LogFacade.Log("Revert to previous state: " + PreviousState.StateType.Name);
            ActiveState = PreviousState;
            await HandleStateResult(await ActiveState.State.OnEntry(Context));
        }

        private async Task HandleStateResult(StateResult result) {
            LogFacade.Log("Interpret: " + result);
            if (result.Revert)
                await RevertToPreviousState();
            if (!result.Revert && result.NextState.IsNotNull())
                EnqueueEvent(EventInstance.Create(result.NextState));
        }

        private void EnqueueEvent(IEventInstance instance) {
            QueuedEvents.Enqueue(instance);
            LogFacade.Log("Queued events now " + QueuedEvents.Count);
        }

        public bool HasPreviousState { get { return PreviousState.IsNotNull(); } }

        public IStateMachine<TContext> BeReflexive() {
            Stable();
            // Assert event name is not null
            return this.Fluently(_ => {
                CurrentStateDefinition.HandledEvents[CurrentEventName] = CurrentStateDefinition.StateType;
                CurrentEventName = null;
            });
        }

        public IStateMachine<TContext> WithContext(TContext ctx) {
            return this.Fluently(_ => Context = new StateMachineContext<TContext> { EnclosingMachine = this, ExecutionContext = ctx });
        }

        public IStateMachine<TContext> Complete() {
            return this.Fluently(_ => {
                if (CurrentStateDefinition.IsNotNull()) {
                    States.Add(CurrentStateDefinition);
                    CurrentStateDefinition = null;
                }
            });
        }

        public IStateMachine<TContext> ForAllEnter<TState>(string eventName) where TState : IState<TContext> {
            return this.Fluently(_ => {
                States.ForEach(s => s.HandledEvents[eventName] = typeof(TState));
            });
        }

        public IStateMachine<TContext> DiscardUnknownEvents() {
            return this.Fluently(_ => IgnoreUnknownEvents = true);
        }

        public IStateMachine<TContext> DIContainer(IContainer ctr) {
            return this.Fluently(_ => Container = ctr);
        }

        public IStateMachineContext<TContext> Context { get; private set; }

        private void Stable() {
            Assert.False(CurrentStateDefinition.IsNull(), () => "Machine is unstable");
        }

        private string CurrentEventName { get; set; }

        private StateDefinition CurrentStateDefinition { get; set; }

        private StateDefinition ActiveState { get; set; }

        private StateDefinition PreviousState { get; set; }

        private bool IgnoreUnknownEvents { get; set; }

        public IContainer Container { get; private set; }

        private class StateDefinition {

            internal StateDefinition(IContainer ctr) {
                HandledEvents = new Dictionary<string, Type>();
                Container = ctr;
            }
            internal Dictionary<string, Type> HandledEvents { get; private set; }
            internal bool Singleton { get; set; }
            internal Type StateType { get; set; }
            internal bool IsBounceState { get; set; }
            internal string Name { get { return StateType.Name;  } }
            internal IState<TContext> State {
                get {
                    IState<TContext> state = CachedState;
                    if (state.IsNull()) {
                        state = Container.IsNotNull() ? Container.Resolve<IState<TContext>>(StateType.Name) : Activator.CreateInstance(StateType) as IState<TContext>;
                        if (IsSingleton)
                            CachedState = state;
                        state.Bouncer = IsBounceState;
                    }
                    return state;
                }
            }
            internal bool IsStartState { get; set; }
            internal bool IsSingleton { get; set; }
            internal Type EventReceived(string name, bool ignoreNonMatch) {
                var exists = HandledEvents.ContainsKey(name);
                Assert.True(exists || ignoreNonMatch, () => "Received an event that could not be handled " + name);
                return exists ? HandledEvents[name] : null;
            }
            private IState<TContext> CachedState { get; set; }
            private IContainer Container { get; set; }
        }

    }
}
