using System.Threading.Tasks;
using FSM;
using Infra;
using Quorum.Integration;
using Quorum.Payloads;

namespace Quorum.States {

    public class ReceivingConfigurationState : BaseState<IExecutionContext> {

        public IConfiguration Configuration { get; set; }

        public IPayloadParser Parser { get; set; }

        public override Task<StateResult> OnEntry(IStateMachineContext<IExecutionContext> context) {
            var request = Parser.As<ConfigurationBroadcast>(context.CurrentEvent.Payload);
            Configuration.LocalSet(Constants.Configuration.Nexus.Key, string.Join(",", request.QuorumMembers));
            // Persist if possible
            return Task.FromResult(StateResult.Create(nextState: EventNames.Discovery));
        }

    }

}
