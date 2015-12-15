#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FSM {

    public abstract class BaseState<TContext> : IState<TContext> where TContext : IMinimalContext {

        protected Dictionary<string, Action<TContext>> ReflexiveHandlers { get; private set; }

        public BaseState() {
            ReflexiveHandlers = new Dictionary<string, Action<TContext>>();
            Interruptable = true;
        }

        public virtual async Task<StateResult> OnEntry(IStateMachineContext<TContext> context) {
            return StateResult.None;
        }

        public virtual async Task OnReflexiveEntry(IStateMachineContext<TContext> context) {
            if (ReflexiveHandlers.ContainsKey(context.CurrentEvent.EventName))
                ReflexiveHandlers[context.CurrentEvent.EventName](context.ExecutionContext);
        }

        public virtual async Task<StateResult> OnExit(IStateMachineContext<TContext> context) {
            return StateResult.None;
        }

        public virtual async Task<StateResult> Execute(IStateMachineContext<TContext> context, IEventInstance anEvent) {
            return StateResult.None;
        }

        public virtual bool Interruptable { get; set; }

        public bool Bouncer { get; set; }

    }
}
