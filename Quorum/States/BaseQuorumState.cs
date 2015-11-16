using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FSM;
using Infra;

namespace Quorum.States {
    
    public abstract class BaseQuorumState : BaseState<IExecutionContext> {

        private const int MiniPause = 25;

        public IMasterWorkAdapter Worker { get; set; }

        protected void EnsureWorkerActive(IExecutionContext ctx) {
            var worker = GetWorker(ctx) ?? new WorkerContainer { Processor = Worker, ExecutionContext = ctx };
            if (worker.ProcessingTask.IsNull() || worker.ProcessingTask.Status != TaskStatus.Running) {
                worker.Processor.WorkUnitExecuted = worker.WorkerExecutes;
                worker.CancellationToken = new CancellationTokenSource();
                var token = worker.CancellationToken.Token;
                worker.ProcessingTask = Task.Factory.StartNew(() => {
                    worker.Processor.Activated();
                    while (!token.IsCancellationRequested)
                        Task.Delay(100).Wait();
                }, token);
                SetWorker(ctx, worker);
            }
        }

        protected void DeActivateWorker(IExecutionContext ctx) {
            try {
                var worker = GetWorker(ctx);
                if (worker.IsNotNull()) {
                    worker.Processor.DeActivated();
                    worker.CancellationToken.Cancel();
                    worker.ProcessingTask.Wait();
                    SetWorker(ctx, null);
                }
            }
            catch { }
        }

        private void WorkerExecutes() {

        }

        private WorkerContainer GetWorker(IExecutionContext ctx) {
            return (ctx.StateStore.ContainsKey(Constants.Local.MasterStateTaskKey) ? ctx.StateStore[Constants.Local.MasterStateTaskKey] : null) as WorkerContainer;
        }

        private void SetWorker(IExecutionContext ctx, WorkerContainer ctr) {
            ctx.StateStore[Constants.Local.MasterStateTaskKey] = ctr;
        }

        private class WorkerContainer { 

            internal Task ProcessingTask { get; set; }

            internal CancellationTokenSource CancellationToken { get; set; }

            internal IMasterWorkAdapter Processor { get; set; }

            internal IExecutionContext ExecutionContext { get; set; }

            internal void WorkerExecutes() {
                ExecutionContext.WorkerExecutionUnits += 1;
            }
        }

    }

}
