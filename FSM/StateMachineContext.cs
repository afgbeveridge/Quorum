#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion

namespace FSM {

    public class StateMachineContext<TContext> : IStateMachineContext<TContext> where TContext : IMinimalContext {

        public StateMachineContext() {  }

        public StateMachineContext(TContext ctx, IStateMachineChannel<TContext> mc) {
            ExecutionContext = ctx;
            EnclosingMachine = mc;
        }

        public TContext ExecutionContext { get; internal set; }

        public IStateMachineChannel<TContext> EnclosingMachine { get; internal set; }

        public IEventInstance CurrentEvent { get; set; }

    }

}
