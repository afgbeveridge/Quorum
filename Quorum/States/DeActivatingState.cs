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
using System.Configuration;
using Infra;

namespace Quorum.States {

    public class DeActivatingState : BaseQuorumState {

        public IPayloadBuilder Builder { get; set; }

        public IWriteableChannel ChannelPrototype { get; set; }

        public IConfiguration Configuration { get; set; }

        public override async Task<StateResult> OnEntry(IStateMachineContext<IExecutionContext> context) {
            try {
                Interruptable = false;
                await DeActivateWorker(context.ExecutionContext);
                context.ExecutionContext.IsMaster = false;
                await ContactNeighbours(context.ExecutionContext);
                PostObit(context);
            }
            finally {
                Interruptable = true;
            }
            return FinalResult;
        }

        protected virtual StateResult FinalResult {
            get {
                return StateResult.None;
            }
        }

        protected virtual void PostObit(IStateMachineContext<IExecutionContext> context) { }

        private async Task ContactNeighbours(IExecutionContext ctx) {
            var staticNeighbours = ctx.Network.Neighbours.Select(n => n.Name);
            Task[] queries = staticNeighbours.Select(s => SendObit(new TaskState { NeighbourName = s, IsMaster = ctx.IsMaster, LocalAddress = ctx.LocalAddress })).ToArray();
            await Task.WhenAll(queries);
        }

        // A null neighbour is returned if cannot be reached
        private async Task SendObit(TaskState cur) {
            // Timeout is config
            LogFacade.Instance.LogInfo("Send obit to " + cur.NeighbourName);
            var content = Builder.Create(new DeathAnnouncement { IsMaster = cur.IsMaster, Name = cur.LocalAddress });
            await ChannelPrototype.NewInstance().Write(cur.NeighbourName, content, Configuration.Get<int>(Constants.Configuration.ResponseLimit));
        }

        private class TaskState {
            internal string NeighbourName { get; set; }
            internal bool IsMaster { get; set; }
            internal string LocalAddress { get; set; }
        }
    }
}
