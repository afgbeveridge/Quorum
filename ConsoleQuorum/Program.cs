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

namespace ConsoleQuorum {
    
    public class Program {

        private static Dictionary<string, Func<bool>> Handlers = new Dictionary<string, Func<bool>> { 
            { "q", Query },
            { "x", Exit }
        };

        private static IStateMachine<IExecutionContext> Machine { get; set; }

        private static IContainer Container { get; set; }

        private static IHttpEventListener<IExecutionContext> Listener { get; set; }

        private static Timer DiscoveryTimer { get; set; }

        public static void Main(string[] args) {
            Task<IStateMachine<IExecutionContext>> task = null;
            try {
                Configure();
                var builder = new Builder();
                Machine = builder.Create();
                builder.Register<IMasterWorkAdapter, NullWorkerAdapter>();
                Container = builder.AsContainer();
                task = Machine.Start();
                SpinUpListener();
                bool cont = true;
                while (cont) {
                    var k = Console.ReadLine();
                    if (k != null && Handlers.ContainsKey(k))
                        cont = Handlers[k]();
                }
            }
            finally {
                DiscoveryTimer.Stop();
                DiscoveryTimer.Dispose();
                Listener.UnInitialize();
                Machine.Trigger(EventInstance.Create(EventNames.Die)).Wait();
                task.Wait();
            }
        }

        private static void Configure() {
            JsonConvert.DefaultSettings = (() => {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter());
                return settings;
            });
            LogFacade.Adapter = new NLogLogger().Configure();
            DiscoveryTimer = new Timer(new Infra.Configuration().Get<int>(Constants.Configuration.DiscoveryPeriodMs));
            // Hook up the Elapsed event for the timer. 
            DiscoveryTimer.Elapsed += (src, args) => Machine.Trigger(EventInstance.Create(EventNames.Discovery));
            DiscoveryTimer.AutoReset = true;
            DiscoveryTimer.Enabled = true;
        }

        private static bool Query() {
            Machine.Trigger(EventInstance.Create(EventNames.Query, "{ \"TypeHint\": \"QueryRequest\", \"Requester\": \"localhost\" }"));
            return true;
        }

        private static bool Exit() {
            return false;
        }

        private static void SpinUpListener() {
            Listener = Container.Resolve<IHttpEventListener<IExecutionContext>>();
            Listener.Machine = Machine;
            Listener.Initialize();
        }

    }
}
