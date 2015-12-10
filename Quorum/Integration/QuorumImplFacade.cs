#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Threading.Tasks;
using FSM;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Infra;
using System.Timers;
using Quorum.Payloads;


namespace Quorum.Integration {

    public class QuorumImplFacade {

        private const int MaxDeathCycles = 100;
        private const int DeathWait = 100;

        public QuorumImplFacade WithBuilder(Builder bldr) {
            return this.Fluently(_ => BuildHelper = bldr);
        }

        public QuorumImplFacade OnTransport(TransportType type) {
            return this.Fluently(_ => ActiveDisposition.Current = type);
        }

        public QuorumImplFacade OnTransport(string type) {
            return this.Fluently(_ => ActiveDisposition.Initialise(type));
        }

        public QuorumImplFacade WithLogOptions(LoggingOptions options) {
            return this.Fluently(_ => LogOptions = options);
        }

        public QuorumImplFacade Start<TWorker>() where TWorker : IMasterWorkAdapter {
            return Start(typeof(TWorker));
        }

        public QuorumImplFacade Start(Type impl) {
            return this.Fluently(_ => {
                Assert.False(BuildHelper.IsNull(), () => "You must call WithBuilder before Start()");
                ConfigureLogging();
                Machine = BuildHelper.Create();
                Container = BuildHelper.AsContainer();
                SpinUpListener();
                Configure();
                BuildHelper.Register<IMasterWorkAdapter>(impl);
                MachineTask = Machine.Start();
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

        private Builder BuildHelper { get; set; }

        public QuorumImplFacade ConfigureLogging(LoggingOptions options = null) {
            return this.Fluently(_ => {
                if (LogFacade.Instance.Adapter.IsNull())
                    LogFacade.Instance.Adapter = new NLogLogger().Configure(options ?? LogOptions);
            });
        }

        private void Configure() {
            // This is cached up front as the interrogation is quite slow, and the results are returned with any query
            HardwareDetails.Interrogate();
            JsonConvert.DefaultSettings = (() => {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter());
                return settings;
            });
            DiscoveryTimer = new Timer(new Infra.Configuration().Get<int>(Constants.Configuration.DiscoveryPeriodMs));
            // Hook up the Elapsed event for the timer. 
            DiscoveryTimer.Elapsed += (src, args) => Machine.Trigger(EventInstance.Create(EventNames.Discovery));
            DiscoveryTimer.AutoReset = true;
            DiscoveryTimer.Enabled = true;
        }

        private Timer DiscoveryTimer { get; set; }

        private void SpinUpListener() {
            Listener = Container.Resolve<IExposedEventListener<IExecutionContext>>();
            Listener.Machine = Machine;
            Listener.Initialize();
        }

        public IStateMachine<IExecutionContext> Machine { get; private set; }

        private IContainer Container { get; set; }

        private IExposedEventListener<IExecutionContext> Listener { get; set; }

        private Task MachineTask { get; set; }

        private LoggingOptions LogOptions { get; set; }

    }
}
