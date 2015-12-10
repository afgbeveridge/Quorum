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
using Quorum;
using System.Threading;
using Infra;

namespace ConsoleQuorum {

    public class NullWorkerAdapter : BaseMasterWorkAdapter {

        protected override async Task<bool> Work() {
            await Task.Delay(2000);
            Console.WriteLine("Working.....");
            if (WorkUnitExecuted.IsNotNull())
                WorkUnitExecuted();
            return ContinueExecuting;
        }

        protected override async Task Stopping() {
            await Task.Delay(10);
            Console.WriteLine("Stopping.....");
        }

        protected override void Stopped() {
            Console.WriteLine("Stopped.");
        }

    }
}
