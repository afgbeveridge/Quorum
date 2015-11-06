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

    public class DeathState : BaseQuorumState {

        public IPayloadBuilder Builder { get; set; }

        public IWriteableChannel ChannelPrototype { get; set; }

        public IConfiguration Configuration { get; set; }

        public override async Task<StateResult> OnEntry(IStateMachineContext<IExecutionContext> context) {
            StateResult result = StateResult.None;
            try {
                Interruptable = false;
                DeActivateWorker(context.ExecutionContext);
                ContactNeighbours(context.ExecutionContext);
            }
            finally {
            }
            return result;
        }

        private void ContactNeighbours(IExecutionContext ctx) {
            var staticNeighbours = ctx.Network.Neighbours.Select(n => n.Name);
            IEnumerable<Task> queries = staticNeighbours.Select(s => Task.Factory.StartNew(SendObit, new TaskState { NeighbourName = s, IsMaster = ctx.IsMaster, LocalAddress = ctx.LocalAddress }));
            Task.WaitAll(queries.ToArray());
        }

        // A null neighbour is returned if cannot be reached
        private void SendObit(object state) {
            // Timeout is config
            TaskState cur = (TaskState)state;
            LogFacade.Log("Send obit to " + cur.NeighbourName);
            var content = Builder.Create(new DeathAnnouncement { IsMaster = cur.IsMaster, Name = cur.LocalAddress });
            var task = ChannelPrototype.NewInstance().Write(cur.NeighbourName, content, Configuration.Get<int>(Constants.Configuration.ResponseLimit));
            task.Wait();
        }

        private class TaskState {
            internal string NeighbourName { get; set; }
            internal bool IsMaster { get; set; }
            internal string LocalAddress { get; set; }
        }
    }
}
