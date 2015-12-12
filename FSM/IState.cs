#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSM {

    public interface IState<TContext> {
        Task<StateResult> OnEntry(IStateMachineContext<TContext> context);
        Task OnReflexiveEntry(IStateMachineContext<TContext> context);
        Task<StateResult> OnExit(IStateMachineContext<TContext> context);
        bool Interruptable { get; set; }
        bool Bouncer { get; set; }
        Task<StateResult> Execute(IStateMachineContext<TContext> context, IEventInstance anEvent);
    }

}
