using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuorumWindowsService;

namespace ServiceRunner {
    
    class Program {

        static void Main(string[] args) {
            var svc = new QuorumAwareWindowsService();
            svc.Debug(Effect.Start);
            Console.WriteLine("Running in debug mode");
            Console.ReadLine();
            svc.Debug(Effect.Stop);
        }

    }
}
