using Quorum.States;
using Quorum.Services;
using Quorum.Payloads;
using NSubstitute;
using NUnit.Framework;
using FSM;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Quorum.Tests {

    [TestFixture]
    public class BasicStateMachineTests : BaseQuorumTestFixture {

        [SetUp]
        public void Init() {
            Booter = new Builder(false);
        }

        [TearDown]
        public void Cleanup() {
            Booter.Destroy();
        }

        [Test]
        public async Task QuorumMachine_ShouldStartWithDefaultStates() {
            var mc = await CreateAndStartMachine(Booter, checkOnce: false);
            mc.CurrentState.Name.Should().Be(typeof(DiscoveryState).Name);
        }

        [Test]
        public async Task QuorumMachine_ShouldBeElectedAfterDefaultDiscovery() {
            var mc = await CreateAndStartMachine(Booter);
            mc.CurrentState.Name.Should().Be(typeof(MasterState).Name);
        }

        [Test]
        public async Task QuorumMachine_ShouldDieWhenAsked() {
            var mc = await CreateAndStartMachine(Booter);
            await TriggerThenCheck(mc, EventNames.Die);
            mc.CurrentState.Name.Should().Be(typeof(DeathState).Name);
            CheckStateStore(mc);
        }

        [Test]
        public async Task QuorumMachine_ShouldBeQuiescentWhenAsked() {
            var mc = await CreateAndStartMachine(Booter);
            await TriggerThenCheck(mc, EventNames.Quiescent);
            mc.CurrentState.Name.Should().Be(typeof(QuiescentState).Name);
            CheckStateStore(mc);
        }

        [Test]
        public async Task QuorumMachine_ShouldAbdicateWhenRequested() {
            var mc = await CreateAndStartMachine(Booter);
            await TriggerThenCheck(mc, EventNames.Abdication);
            mc.CurrentState.Name.Should().Be(typeof(QuiescentState).Name);
            CheckStateStore(mc);
        }

        [Test]
        public async Task QuorumMachine_ShouldElectAfterMadePretender() {
            var mc = await CreateAndStartMachine(Booter);
            await TriggerThenCheck(mc, EventNames.Abdication);
            await TriggerThenCheck(mc, EventNames.MakePretender);
            await TriggerThenCheck(mc, EventNames.Discovery);
            mc.CurrentState.Name.Should().Be(typeof(MasterState).Name);
        }

        [Test]
        public async Task QuorumMachine_ShouldRemainQuiescentWhenMadeInEligible() {
            var mc = await CreateAndStartMachine(Booter);
            await TriggerThenCheck(mc, EventNames.Abdication);
            await TriggerThenCheck(mc, EventNames.Discovery);
            mc.CurrentState.Name.Should().Be(typeof(QuiescentState).Name);
            CheckStateStore(mc);
        }

        // This is a cheat, to test NSubstitute only
        [Test]
        public async Task QuorumMachine_EnsureUltraBasicNSubstituteOperationWithAsync() {
            var cs = Substitute.For<ICommunicationsService>();
            cs.DiscoverExcept(Arg.Any<string>()).Returns(Task.FromResult<IEnumerable<Neighbour>>(new[] { NeighbourFactory.CreateNeighbour("B")}));
            var peers = await cs.DiscoverExcept("A");
            peers.Should().NotBeNull("Faked a return");
            peers.Should().HaveCount(1, "Expected one neighbour");
            peers.First().Name.Should().BeEquivalentTo("B", "Because the name was set as B");
        }

        // TODO: Mock out the whole of builder container; just use a dictionary

        private Builder Booter { get; set; }

        private void CheckStateStore(IStateMachine<IExecutionContext> mc) {
            mc.Context.ExecutionContext.StateStore.ContainsKey(Constants.Local.MasterStateTaskKey).Should().BeTrue("Worker must be referenced in context");
            mc.Context.ExecutionContext.StateStore[Constants.Local.MasterStateTaskKey].Should().BeNull("Worker must be null in non master state");
        }

    }

}
