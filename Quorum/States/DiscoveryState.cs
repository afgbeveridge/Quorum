﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FSM;
using Quorum.Services;
using Quorum.Payloads;
using System.Configuration;
using Infra;

namespace Quorum.States {
    
    public class DiscoveryState : BaseState<IExecutionContext> {

        public IDiscoveryService DiscoveryService { get; set; }

        public override async Task<StateResult> OnEntry(IStateMachineContext<IExecutionContext> context) {
            bool reversionPossible = AllowInterrupt;
            StateResult result = StateResult.None;
            try {
                Interruptable = AllowInterrupt;
                var neighbours = DiscoveryService.DiscoverExcept(context.ExecutionContext.HostName);
                context.ExecutionContext.Network.Neighbours.Clear();
                context.ExecutionContext.Network.Neighbours.AddRange(neighbours);
                if (!neighbours.Any()) {
                    reversionPossible = false;
                    LogFacade.Instance.LogInfo("No neighbours, but ineligible for election? " + context.ExecutionContext.InEligibleForElection);
                    result.NextState = context.ExecutionContext.InEligibleForElection ? EventNames.Quiescent : EventNames.Elected;
                }
                // No masters or potential split brain
                else if (!neighbours.Any(n => n.IsMaster) || context.ExecutionContext.IsMaster) { // This needs to be non concrete i.e. no reference to EventInstance
                    LogFacade.Instance.LogInfo("Neighbours, but need an election");
                    result.NextState = EventNames.RequestElection;
                    // Cannot revert from this state
                    reversionPossible = false;
                }
                else if (neighbours.Any(n => n.IsMaster)) {
                    LogFacade.Instance.LogInfo("Neighbours, but a master in one of them");
                    result.NextState = EventNames.Quiescent;
                }
            }
            finally {
                Interruptable = true;
                AllowInterrupt = true;
                context.ExecutionContext.Network.LastChecked = DateTime.Now;
            }
            result.Revert = reversionPossible && context.EnclosingMachine.HasPreviousState;
            return result;
        }

        private bool AllowInterrupt { get; set; }

    }

}