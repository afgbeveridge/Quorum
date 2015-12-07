using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quorum;
using System.Threading;
using Infra;

namespace WorkerAdapterExample {

    // Replace the implementation of this class with whatever implementation you wish

    public class WorkerAdapter : BaseMasterWorkAdapter {

        protected override async Task<bool> Work() {
            await Task.Delay(10);
            return false;
        }

        protected override async Task Stopping() {
            LogFacade.Instance.LogInfo("Stopping...");
            await Task.Delay(10);
        }

        protected override void Stopped() {
            LogFacade.Instance.LogInfo("Stopped.");
        }

    }
}

