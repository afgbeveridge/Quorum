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
using FSM;
using Quorum.Integration;
using Quorum.Payloads;

namespace Quorum.States {

    public class DeathAnnouncementState : BaseState<IExecutionContext> {

        public IPayloadParser Parser { get; set; }

        public override async Task<StateResult> OnEntry(IStateMachineContext<IExecutionContext> context) {
            var obit = Parser.As<DeathAnnouncement>(context.CurrentEvent.Payload);
            return new StateResult { Revert = !obit.IsMaster, NextState = obit.IsMaster ? EventNames.RequestElection : null };
        }
    }
}
