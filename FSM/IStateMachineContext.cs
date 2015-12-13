﻿#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion

namespace FSM {

    public interface IStateMachineContext<TContext> {
        TContext ExecutionContext { get; }
        IStateMachineChannel<TContext> EnclosingMachine { get; }
        IEventInstance CurrentEvent { get; set; }
    }
}
