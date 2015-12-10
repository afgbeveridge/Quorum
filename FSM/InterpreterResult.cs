﻿#region License
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
    
    public class InterpreterResult<TContext> {
        public string EventName { get; set; }
        public Type ExecutableStateType { get; set; }
    }
}
