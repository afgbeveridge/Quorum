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
            //LogFacade.Log("Discovery requested? " + RequestedDiscovery);
            var result = AnalyzeDiscovery(context); //!RequestedDiscovery ? StateResult.Create(nextState: EventNames.Discovery) : AnalyzeDiscovery(context.ExecutionContext);
            //RequestedDiscovery = !RequestedDiscovery;
            return result;
        }

        private StateResult AnalyzeDiscovery(IStateMachineContext<IExecutionContext> ctx) {
            // TODO: Include self in choice list
            var strength = Configuration.Get<string>(Constants.Configuration.MachineStrength.Key);
            var actualStrength = String.IsNullOrEmpty(strength) ? MachineStrength.Compute : (MachineStrength)Enum.Parse(typeof(MachineStrength), strength, true);
            Neighbour choice = Adjudicator.Choose(ctx.ExecutionContext.Network.Neighbours, new Neighbour {
                IsMaster = ctx.ExecutionContext.IsMaster,
                Name = ctx.ExecutionContext.HostName,
                UpTime = ctx.EnclosingMachine.UpTime,
                Strength = actualStrength
            });
            var selfElected = choice.IsNull() || choice.Name == ctx.ExecutionContext.HostName;
            return StateResult.Create(nextState: selfElected ? EventNames.Elected : EventNames.Quiescent);
        }

    }

}
