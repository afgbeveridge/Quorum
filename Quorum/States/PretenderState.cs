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
using Quorum.Integration;
using Quorum.Payloads;
using System.Configuration;
using Infra;

namespace Quorum.States {

    public class PretenderState : BaseState<IExecutionContext>{

        public override async Task<StateResult> OnEntry(IStateMachineContext<IExecutionContext> context) {
            context.ExecutionContext.InEligibleForElection = false;
            LogFacade.Instance.LogInfo("Marking self as eligible for election");
            return StateResult.None;
        }

    }
}
