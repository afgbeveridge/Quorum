﻿using System;
using Quorum.Payloads;
using System.Threading.Tasks;
using FSM;
using NSubstitute;

namespace Quorum.Tests {

    public class EmptyWorker : BaseMasterWorkAdapter {

        protected async override Task<bool> Work() {
            return ContinueExecuting;
        }

        protected async override Task Stopping() {
        }

        protected override void Stopped() {

        }

    }


    public static class NeighbourFactory {

        public static Neighbour CreateNeighbour(IStateMachineContext<IExecutionContext> ctx, Action<Neighbour> f = null) {
            Neighbour n = Neighbour.Fabricate(ctx);
            if (f != null)
                f(n);
            return n;
        }

        public static Neighbour CreateNeighbour(string name) {
            return Neighbour.NonRespondingNeighbour(name);
        }

        public static IStateMachineContext<IExecutionContext> FakeContext {
            get {
                var ctx = Substitute.For<IExecutionContext>();
                ctx.HostName.Returns("Exterior");
                ctx.IsMaster.Returns(false);
                var mc = Substitute.For<IStateMachineChannel<IExecutionContext>>();
                mc.UpTime.Returns(0d);
                return new StateMachineContext<IExecutionContext>(ctx, mc);
            }
        }

    }
}
