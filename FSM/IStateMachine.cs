#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FSM {

    public interface IStateMachine<TContext> : IStateMachineChannel<TContext> where TContext : IMinimalContext {
        IStateMachine<TContext> DefineState<TState>() where TState : IState<TContext>;
        IStateMachine<TContext> AsStartState();
        IStateMachine<TContext> Singleton();
        IStateMachine<TContext> On(string eventName);
        IStateMachine<TContext> BeReflexive();
        IStateMachine<TContext> TransitionTo<TState>() where TState : IState<TContext>;
        Task<IStateMachine<TContext>> Start();
        IStateMachine<TContext> Stop();
        IStateMachine<TContext> WithContext(TContext ctx);
        IStateMachine<TContext> Complete();
        IStateMachine<TContext> ForAllEnter<TState>(string eventName) where TState : IState<TContext>;
        IStateMachine<TContext> DiscardUnknownEvents();
        IStateMachine<TContext> DIContainer(IContainer ctr);
        IStateMachine<TContext> MarkAsBounceState();
        IStateMachineContext<TContext> Context { get; }
        IContainer Container { get; }
        IEnumerable<IStateDefinition<TContext>> ConfiguredStates { get; }
        Task CheckQueuedEvents();
    }

}
