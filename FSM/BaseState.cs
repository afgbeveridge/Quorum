﻿#region License
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

        public virtual Task<StateResult> OnEntry(IStateMachineContext<TContext> context) {
            return Task.FromResult(StateResult.None);
        }

        public virtual Task OnReflexiveEntry(IStateMachineContext<TContext> context) {
            if (ReflexiveHandlers.ContainsKey(context.CurrentEvent.EventName))
                ReflexiveHandlers[context.CurrentEvent.EventName](context.ExecutionContext);
            return Task.FromResult(0);
        }

        public virtual Task<StateResult> OnExit(IStateMachineContext<TContext> context) {
            return Task.FromResult(StateResult.None);
        }

        public virtual Task<StateResult> Execute(IStateMachineContext<TContext> context, IEventInstance anEvent) {
            return Task.FromResult(StateResult.None);
        }

        public virtual bool Interruptable { get; set; }

        public bool Bouncer { get; set; }

    }
}
