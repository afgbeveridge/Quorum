using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSM {

    public interface IStateMachine<TContext> : IStateMachineChannel<TContext> {
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
    }

}
