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
    
    public class StateResult {

        public bool Revert { get; set; }

        public string NextState { get; set; }

        public static StateResult None {
            get {
                return new StateResult();
            }
        }

        public override string ToString() {
            return "State result: revert? " + Revert + ", Next State: " + NextState;
        }

        public static StateResult Create(bool revert = false, string nextState = null) {
            return new StateResult { Revert = revert, NextState = nextState };
        }

    }
}
