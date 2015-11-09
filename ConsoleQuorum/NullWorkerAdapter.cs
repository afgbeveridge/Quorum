using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quorum;
using System.Threading;
using Infra;

namespace ConsoleQuorum {
    
    public class NullWorkerAdapter : IMasterWorkAdapter {

        private bool Finish { get; set; }

        public async Task Activated() {
            Finish = false;
            while (!Finish) {
                await Task.Delay(5000);
                Finish
                    .IfTrue(() => Console.WriteLine("Won't continue....."))
                    .IfFalse(() => Console.WriteLine("Working....."));
            }
        }

        public async Task DeActivated() {
            Finish = true;
            await Task.Delay(20);
            Console.WriteLine("Stopping.....");
        }
    }
}
