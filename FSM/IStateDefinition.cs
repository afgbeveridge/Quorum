#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;

namespace FSM {

    public interface IStateDefinition<TContext> where TContext : IMinimalContext {
        Dictionary<string, Type> HandledEvents { get; }
        Type StateType { get; }
        bool IsBounceState { get; }
        string Name { get; }
        bool IsStartState { get; }
        bool IsSingleton { get; }
        IState<TContext> CachedState { get; }
    }
}
