#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion

namespace FSM {

    public interface IDirectlyExecutableState<TContext> {

        StateResult Execute(IStateMachineContext<TContext> context, IEventInstance anEvent);

    }
}
