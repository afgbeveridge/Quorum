﻿#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System.Threading.Tasks;
using FSM;
using Quorum.Integration;
using Quorum.Payloads;

namespace Quorum.States {

    public class DeathAnnouncementState : BaseState<IExecutionContext> {

        public IPayloadParser Parser { get; set; }

        public override Task<StateResult> OnEntry(IStateMachineContext<IExecutionContext> context) {
            var obit = Parser.As<DeathAnnouncement>(context.CurrentEvent.Payload);
            return Task.FromResult(new StateResult { Revert = !obit.IsMaster, NextState = obit.IsMaster ? EventNames.RequestElection : null });
        }
    }
}
