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

    public class StateMachineContext<TContext> : IStateMachineContext<TContext> {

        public TContext ExecutionContext { get; internal set; }

        public IStateMachineChannel<TContext> EnclosingMachine { get; internal set; }

        public IEventInstance CurrentEvent { get; set; }

    }

}
