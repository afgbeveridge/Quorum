using System;
using NUnit.Framework;
using Infra;
using NSubstitute;
using FSM;
using System.Threading.Tasks;

namespace FSM.Tests {

    [TestFixture]
    public class StateMachine {

        [Test]
        public async Task StartWithNoStates() {
            await AsyncAssert.Throws<ApplicationException>(async () => {
                await CreateMachine.Start();
            });
        }

        [Test]
        public async Task StartWithNoStartState() {
            await AsyncAssert.Throws<ApplicationException>(async () => {
                var mc = CreateMachine;
                mc.DefineState<EmptyState>();
                await CreateMachine.Start();
            });
        }

        [Test]
        public async Task UniStateStart() {
            var mc = CreateMachine;
            mc.DefineState<EmptyState>().AsStartState();
            await mc.Start();
        }

        private IStateMachine<EmptyClass> CreateMachine {
            get {
                return new SimpleStateMachine<EmptyClass>(Substitute.For<IEventQueue>(), Substitute.For<IEventStatistician>(), false);
            }
        }

    }
}
