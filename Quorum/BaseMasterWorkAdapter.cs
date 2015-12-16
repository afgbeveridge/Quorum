#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Threading.Tasks;
using System.Threading;
using Infra;

namespace Quorum {

    public abstract class BaseMasterWorkAdapter : IMasterWorkAdapter {

        private SemaphoreSlim Region { get; set; }

        public BaseMasterWorkAdapter() {
            Region = new SemaphoreSlim(1, 1);
        }

        private bool Finish { get; set; }

        public async Task Activated() {
            LogFacade.Instance.LogInfo("Activating worker adapter");
            Finish = false;
            int delay = 1;
            while (!Finish) {
                await Task.Delay(delay);
                await Region.WaitAsync();
                if (!Finish) {
                    var result = await Work();
                    Finish = ! result.Continue;
                    delay = result.BackOffPeriod.HasValue ? result.BackOffPeriod.Value : delay;
                }
                Region.Release();
            }
            LogFacade.Instance.LogInfo("Finishing worker adapter");
            Stopped();
        }

        protected abstract Task<WorkResult> Work();

        protected abstract Task Stopping();

        protected abstract void Stopped();

        public async Task DeActivated() {
            await Region.WaitAsync();
            Finish = true;
            await Stopping();
            Region.Release();
            await Region.WaitAsync();
            Region.Release();
        }

        public Action WorkUnitExecuted { get; set; }

        protected bool ContinueExecuting {  get { return true; } }

        public class WorkResult {
            public bool Continue { get; set; }
            public int? BackOffPeriod { get; set; }
            public static WorkResult NonCommittal {
                get {
                    return new WorkResult { Continue = true };
                }
            }
        }
    }

}
