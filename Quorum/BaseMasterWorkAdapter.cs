﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quorum;
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
            Finish = false;
            while (!Finish) {
                await Region.WaitAsync();
                if (!Finish)
                    Finish = await Work();
                Region.Release();
            }
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
    }

}