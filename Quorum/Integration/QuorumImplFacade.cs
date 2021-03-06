﻿#region License
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
using Quorum.States;
using System.Linq;


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

        public QuorumImplFacade WithModifiedLogOptions(Action<LoggingOptions> f) {
            return this.Fluently(_ => LogOptions = DeriveLoggingOptions(f) );
        }

        public QuorumImplFacade Start<TWorker>() where TWorker : IMasterWorkAdapter {
            return Start(typeof(TWorker));
        }

        public QuorumImplFacade Start(Type impl) {
            return this.Fluently(_ => {
                DBC.False(BuildHelper.IsNull(), () => "You must call WithBuilder before Start()");
                ConfigureLogging(LogOptions ?? DeriveLoggingOptions());
                Machine = BuildHelper.Create();
                Container = BuildHelper.AsContainer();
                CheckStability();
                SpinUpListener();
                Configure();
                BuildHelper.Register<IMasterWorkAdapter>(impl);
                // Caution as we are running an async method in a synchronous manner
                Task.Run(() => Machine.Start()).Wait();
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
                    LogFacade.Instance.Adapter = new NLogLogger().Configure(options ?? LogOptions ?? DeriveLoggingOptions());
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
            DiscoveryTimer = new Timer(new Infra.Configuration().WithAppropriateOverrides().Get(Constants.Configuration.DiscoveryPeriodMs));
            // Hook up the Elapsed event for the timer. 
            DiscoveryTimer.Elapsed += (src, args) => Machine.Trigger(EventInstance.Create(EventNames.Discovery));
            DiscoveryTimer.AutoReset = true;
            DiscoveryTimer.Enabled = true;
        }

        public LoggingOptions DeriveLoggingOptions(Action<LoggingOptions> f = null) {
            var opts = new LoggingOptions {
                MinimalLogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), new Configuration().WithAppropriateOverrides().Get(Constants.Configuration.MinimalLogLevel), true)
            };
            if (f.IsNotNull())
                f(opts);
            return opts;
        }

        private Timer DiscoveryTimer { get; set; }

        private void SpinUpListener() {
            Listener = Container.Resolve<IExposedEventListener<IExecutionContext>>();
            Listener.Machine = Machine;
            Listener.Initialize();
        }

        private void CheckStability() {
            var cfg = Container.Resolve<IConfiguration>().WithAppropriateOverrides();
            if (string.IsNullOrEmpty(cfg.Get(Constants.Configuration.Nexus))) {
                LogFacade.Instance.LogInfo("Machine will enter pending configuration state, awaiting a configuration broadcast");
                Machine.ConfiguredStates.First(d => d.IsStartState).IsStartState = false;
                // We know the name of a state...hmmm..
                Machine.ConfiguredStates.First(d => d.StateType == typeof(PendingConfigurationState)).IsStartState = true;
            }
        }

        public IStateMachine<IExecutionContext> Machine { get; private set; }

        private IContainer Container { get; set; }

        private IExposedEventListener<IExecutionContext> Listener { get; set; }

        private LoggingOptions LogOptions { get; set; }

    }
}
