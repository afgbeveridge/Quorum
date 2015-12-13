#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;

namespace FSM {

    public class InterpreterResult<TContext> {
        public string EventName { get; set; }
        public Type ExecutableStateType { get; set; }
    }
}
