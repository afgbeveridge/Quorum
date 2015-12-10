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
using Infra;

namespace Quorum.States {

    public class ElectionState : BaseState<IExecutionContext> {

        //private bool RequestedDiscovery { get; set; }

        public IElectionAdjudicator Adjudicator { get; set; }

        public INetworkEnvironment Network { get; set; }

        public IConfiguration Configuration { get; set; }

        public override async Task<StateResult> OnEntry(IStateMachineContext<IExecutionContext> context) {
            //LogFacade.Instance.LogInfo("Discovery requested? " + RequestedDiscovery);
            var result = AnalyzeDiscovery(context); //!RequestedDiscovery ? StateResult.Create(nextState: EventNames.Discovery) : AnalyzeDiscovery(context.ExecutionContext);
            //RequestedDiscovery = !RequestedDiscovery;
            return result;
        }

        private StateResult AnalyzeDiscovery(IStateMachineContext<IExecutionContext> ctx) {
            Neighbour choice = Adjudicator.Choose(ctx.ExecutionContext.Network.Neighbours, Neighbour.Fabricate(ctx));
            var selfElected = (choice.IsNull() || choice.Name == ctx.ExecutionContext.HostName) && !ctx.ExecutionContext.InEligibleForElection;
            return StateResult.Create(nextState: selfElected ? EventNames.Elected : EventNames.Quiescent);
        }

    }

}
