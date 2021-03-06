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
using FSM;

namespace Quorum.States {

    public class MasterState : BaseQuorumState {

        public override Task<StateResult> OnEntry(IStateMachineContext<IExecutionContext> context) {
            context.ExecutionContext.IsMaster = true;
            EnsureWorkerActive(context.ExecutionContext);
            return Task.FromResult(StateResult.None);
        }

    }
}
