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
            while (!Finish) {
                await Region.WaitAsync();
                if (!Finish)
                    Finish = ! await Work();
                Region.Release();
            }
            LogFacade.Instance.LogInfo("Finishing worker adapter");
            Stopped();
        }

        protected abstract Task<bool> Work();

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
    }

}
