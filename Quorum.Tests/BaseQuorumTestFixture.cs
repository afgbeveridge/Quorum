using FSM;
using FluentAssertions;
using Infra;
using System.Threading.Tasks;

namespace Quorum.Tests {

    public abstract class BaseQuorumTestFixture {

        protected IStateMachine<IExecutionContext> CreateMachine(Builder bldr) {
            var mc = bldr.Create();
            bldr.Register<IMasterWorkAdapter, EmptyWorker>();
            // Register any old nexus
            bldr.Resolve<IConfiguration>().LocalSet(Constants.Configuration.Nexus.Key, "A,B");
            return mc;
        }

        protected async Task<IStateMachine<IExecutionContext>> CreateAndStartMachine(Builder bldr, bool checkOnce = true) {
            var mc = CreateMachine(bldr);
            return await StartMachine(mc, checkOnce);
        }

        protected async Task<IStateMachine<IExecutionContext>> StartMachine(IStateMachine<IExecutionContext> mc, bool checkOnce = true) {
            await mc.Start();
            mc.CurrentState.Should().NotBeNull("Default machine should have a current state");
            if (checkOnce)
                await mc.CheckQueuedEvents();
            return mc;
        }

        protected async Task TriggerThenCheck(IStateMachine<IExecutionContext> mc, string eventName) {
            await mc.Trigger(EventInstance.Create(eventName));
            await mc.CheckQueuedEvents();
        }


    }
}
