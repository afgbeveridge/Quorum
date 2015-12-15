using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using Quorum.Services;
using Quorum.Payloads;
using FluentAssertions;
using Quorum.States;
using System;

namespace Quorum.Tests {

    [TestFixture]
    public class NeighbourTests : BaseQuorumTestFixture {

        [SetUp]
        public void Init() {
            Booter = new TestBuilder();
        }

        [TearDown]
        public void Cleanup() {
            Booter.Destroy();
        }

        // This is to test NSubstitute only
        [Test]
        public async Task QuorumMachine_EnsureNSubstituteInjectedServiceOperatesCorrectly() {
            AddInterjector<ICommunicationsService>(() => {
                var cs = Substitute.For<ICommunicationsService>();
                cs.DiscoverExcept(Arg.Any<string>()).Returns(Task.FromResult<IEnumerable<Neighbour>>(new[] { NeighbourFactory.CreateNeighbour("B") }));
                return cs;
            });
            Booter.CreateBaseRegistrations();
            var svc = Booter.Resolve<ICommunicationsService>();
            var peers = await svc.DiscoverExcept("A");
            peers.Should().NotBeNull("Faked a return");
            peers.Should().HaveCount(1, "Expected one neighbour");
            peers.First().Name.Should().BeEquivalentTo("B", "Because the name was set as B");
        }

        // If no peers, elect self 
        [Test]
        public async Task QuorumMachine_ShouldSelfElectIfHasNoPeers() {
            AddInterjector<ICommunicationsService>(() => {
                var cs = Substitute.For<ICommunicationsService>();
                cs.DiscoverExcept(Arg.Any<string>()).Returns(Task.FromResult<IEnumerable<Neighbour>>(null));
                return cs;
            });
            var mc = await CreateAndStartMachine(Booter);
            mc.CurrentState.Name.Should().Be(typeof(MasterState).Name);
        }

        // Ensure (by default) shortest up time neighbour elected 
        [Test]
        public async Task QuorumMachine_DefaultAdjudicatorElectsOldestCandidate() {
            var n = FakeNeighbour(_ => _.AbsoluteBootTime = double.MaxValue);
            AddInterjector<ICommunicationsService>(() => {
                var cs = Substitute.For<ICommunicationsService>();
                cs.DiscoverExcept(Arg.Any<string>()).Returns(Task.FromResult<IEnumerable<Neighbour>>(new[] { n }));
                return cs;
            });
            var mc = await CreateAndStartMachine(Booter);
            mc.CurrentState.Name.Should().Be(typeof(ElectionState).Name);
            await mc.CheckQueuedEvents();
            mc.CurrentState.Name.Should().Be(typeof(MasterState).Name);
        }

        // Ensure 'self' transitions to quiescent if a peer is already the master 
        [Test]
        public async Task QuorumMachine_ShouldBeQuiescentIfPeerMasterExists() {
            var n = FakeNeighbour(
                _ => {
                    _.AbsoluteBootTime = double.MaxValue;
                    _.IsMaster = true;
            });
            AddInterjector<ICommunicationsService>(() => {
                var cs = Substitute.For<ICommunicationsService>();
                cs.DiscoverExcept(Arg.Any<string>()).Returns(Task.FromResult<IEnumerable<Neighbour>>(new[] { n }));
                return cs;
            });
            var mc = await CreateAndStartMachine(Booter);
            mc.CurrentState.Name.Should().Be(typeof(QuiescentState).Name);
        }

        private void AddInterjector<TInterface>(Func<object> eval) {
            Booter.Interjectors.Add(typeof(TInterface), eval);
        }

        private Neighbour FakeNeighbour(Action<Neighbour> afterCreation = null) {
            return NeighbourFactory.CreateNeighbour(NeighbourFactory.FakeContext, afterCreation);
        }

        private TestBuilder Booter { get; set; }

    }
}
