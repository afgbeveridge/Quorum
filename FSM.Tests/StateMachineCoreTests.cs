using System;
using NUnit.Framework;
using Infra;
using NSubstitute;
using System.Threading.Tasks;
using FluentAssertions;
using System.Linq;

namespace FSM.Tests {

    [TestFixture]
    public class StateMachineCoreTests {

        [Test]
        public async Task MachineWithNoStates_ShouldAbend() {
            await AsyncAssert.Throws<ApplicationException>(async () => {
                await CreateMachine.Start();
            });
        }

        [Test]
        public async Task MachineWithNoStartState_ShouldAbend() {
            await AsyncAssert.Throws<ApplicationException>(async () => {
                var mc = CreateMachine;
                mc.DefineState<EmptyState>();
                await CreateMachine.Start();
            });
        }

        [Test]
        public async Task MachineWithTrivialStartState_ShouldBeStartable() {
            var mc = CreateMachine;
            mc.DefineState<EmptyState>().AsStartState();
            await mc.Start();
        }

        [Test]
        public async Task States_ShouldBeAutomaticallyClosedDuringDefinition() {
            var mc = CreateMachine;
            mc.DefineState<EmptyState>().AsStartState();
            mc.DefineState<FaultingState>();
            await mc.Start();
            mc.ConfiguredStates.Should().HaveCount(2, "States should be auto completed added");
        }

        [Test]
        public async Task SingletonStates_ShouldGetCached() {
            var mc = CreateMachine;
            mc.DefineState<EmptyState>().AsStartState().Singleton();
            await mc.Start();
            mc.ConfiguredStates.First().CachedState.Should().NotBeNull("A singleton state should be cached after activation");
        }

        [Test]
        public async Task TransientStates_ShouldNotBeCached() {
            var mc = CreateMachine;
            mc.DefineState<EmptyState>().AsStartState();
            await mc.Start();
            mc.ConfiguredStates.First().CachedState.Should().BeNull("Non singleton states should be not cached after activation");
        }

        private IStateMachine<EmptyContext> CreateMachine {
            get {
                return new SimpleStateMachine<EmptyContext>(Substitute.For<IEventQueue>(), Substitute.For<IEventStatistician>(), false)
                    .WithContext(new EmptyContext());
            }
        }

    }
}
