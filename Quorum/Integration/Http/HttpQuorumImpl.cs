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


namespace Quorum.Integration.Http {
    
    public class HttpQuorumImpl {

        private const int MaxDeathCycles = 100;
        private const int DeathWait = 100;

        public HttpQuorumImpl Start<TWorker>() where TWorker : IMasterWorkAdapter {
            return this.Fluently(_ => {
                Configure();
                var builder = new Builder();
                Machine = builder.Create();
                builder.Register<IMasterWorkAdapter, TWorker>();
                Container = builder.AsContainer();
                MachineTask = Machine.Start();
                SpinUpListener();
            });
        }

        public async Task Stop() {
            DiscoveryTimer.Stop();
            DiscoveryTimer.Dispose();
            Listener.UnInitialize();
            await Machine.Trigger(EventInstance.Create(EventNames.Die));
            // As the mere triggering of an event does not imply that it has been processed; we wait, within reason to ensure we are no longer master
            var cycle = 0;
            while (cycle < MaxDeathCycles && Machine.Context.ExecutionContext.IsMaster) {
                await Task.Delay(DeathWait);
                cycle++;
            }
        }

        private void Configure() {
            JsonConvert.DefaultSettings = (() => {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter());
                return settings;
            });
            LogFacade.Instance.Adapter = new NLogLogger().Configure();
            DiscoveryTimer = new Timer(new Infra.Configuration().Get<int>(Constants.Configuration.DiscoveryPeriodMs));
            // Hook up the Elapsed event for the timer. 
            DiscoveryTimer.Elapsed += (src, args) => Machine.Trigger(EventInstance.Create(EventNames.Discovery));
            DiscoveryTimer.AutoReset = true;
            DiscoveryTimer.Enabled = true;
        }

        private Timer DiscoveryTimer { get; set; }

        private void SpinUpListener() {
            Listener = Container.Resolve<IHttpEventListener<IExecutionContext>>();
            Listener.Machine = Machine;
            Listener.Initialize();
        }

        public IStateMachine<IExecutionContext> Machine { get; private set; }

        private IContainer Container { get; set; }

        private IHttpEventListener<IExecutionContext> Listener { get; set; }

        private Task MachineTask { get; set; }

    }
}
