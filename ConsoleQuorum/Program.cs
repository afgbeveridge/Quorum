using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quorum;
using FSM;
using Quorum.Integration.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Infra;
using System.Timers;
using Quorum.Integration.Http;

namespace ConsoleQuorum {
    
    public class Program {

        private static Dictionary<string, Func<bool>> Handlers = new Dictionary<string, Func<bool>> { 
            { "q", Query },
            { "x", Exit }
        };

        private static HttpQuorumImpl Adapter { get; set; }

        public static void Main(string[] args) {
            try {
                Adapter = new HttpQuorumImpl().Start<NullWorkerAdapter>();
                bool cont = true;
                while (cont) {
                    var k = Console.ReadLine();
                    if (k != null && Handlers.ContainsKey(k))
                        cont = Handlers[k]();
                }
            }
            finally {
                "Guard".GuardedExecution(() => {
                    if (Adapter.IsNotNull())
                        Adapter.Stop().Wait();
                });
            }
        }

        private static bool Query() {
            Adapter.Machine.Trigger(EventInstance.Create(EventNames.Query, "{ \"TypeHint\": \"QueryRequest\" }"));
            return true;
        }

        private static bool Exit() {
            return false;
        }

    }
}
