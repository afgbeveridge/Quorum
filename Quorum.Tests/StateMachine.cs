using System;
using NUnit.Framework;
using FSM;
using Infra;
using System.Threading.Tasks;

namespace Quorum.Tests {

    [TestFixture]
    public class StateMachine {

        [OneTimeSetUp]
        public void Init() {
            Booter = new Builder();
            Machine = Booter.Create();
        }

        [OneTimeTearDown]
        public void Cleanup() {
            Booter.Destroy();
        }

        [Test]
        public async Task StartWithDefaultStates() {
            await Machine.Start();
        }

        private Builder Booter { get; set; }

        private IStateMachine<IExecutionContext> Machine { get; set; }

    }

}
