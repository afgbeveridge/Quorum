using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quorum;
using FSM;
using Quorum.Integration;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Infra;
using System.Timers;
using System.DirectoryServices;

namespace ConsoleQuorum {

    public class Program {

        private static Dictionary<string, Func<bool>> Handlers = new Dictionary<string, Func<bool>> { 
            { "q", Query },
            { "x", Exit }
        };

        private static QuorumImplFacade Adapter { get; set; }

        public static void Main(string[] args) {
            try {
                Test();
                Adapter = new QuorumImplFacade()
                    .WithBuilder(new Builder())
                    .OnTransport(args.Any() ? args.First() : null)
                    .Start<NullWorkerAdapter>();
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

        private static void Test() {
            int len = 5;
            string s = "Any old shite";
            s = len + s.Length.ToString().PadLeft(len, '0') + s;
            var b = ASCIIEncoding.Default.GetBytes(s);
            var q = ASCIIEncoding.Default.GetString(b);
            var m = (char)b[0] - '0';
            var f = int.Parse("00004");
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
