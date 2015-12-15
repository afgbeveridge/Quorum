#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Linq;
using System.ServiceProcess;
using Quorum.Integration;
using Quorum;
using Infra;
using System.Reflection;
using System.Threading.Tasks;

namespace QuorumWindowsService {

    public enum Effect { Start, Stop }

    public partial class QuorumAwareWindowsService : ServiceBase {

        private QuorumImplFacade Adapter { get; set; }

        public QuorumAwareWindowsService() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            // Configure logging early to log any worker type load issues
            TaskScheduler.UnobservedTaskException += (sender, exceptionEventArgs) => {
                exceptionEventArgs.SetObserved();
                LogFacade.Instance.LogError("Unhandled exception propagated", exceptionEventArgs.Exception);
            };
            Adapter = new QuorumImplFacade()
                        .WithModifiedLogOptions(opts => opts.RequireConsoleSink = false)
                        .ConfigureLogging()
                        .WithBuilder(new Builder())
                        .Start(GetWorkerImplementation());
        }

        protected override void OnStop() {
            // Caution as we are running an async method in a synchronous manner
            if (Adapter.IsNotNull())
                Task.Run(() => Adapter.Stop()).Wait();
        }

        public void Debug(Effect action) {
            if (action == Effect.Start)
                OnStart(null);
            else
                OnStop();
        }

        // This could be some Infra class, but I'm in YAGNI mode
        private Type GetWorkerImplementation() {
            Type workerType = null;
            var assName = new Configuration().Get<string>("quorum.workerTypeAssembly");
            try {
                DBC.False(assName.IsNull(), () => "You must specify an assembly name in quorum.workerTypeAssembly");
                Assembly assembly = Assembly.Load(assName);
                workerType = assembly.GetExportedTypes().FirstOrDefault(t => t.GetInterfaces().Contains(typeof(IMasterWorkAdapter)) && !t.IsAbstract);
                DBC.False(workerType.IsNull(), () => "No type in " + assName + " implements the worker interface");
                object instance = Activator.CreateInstance(workerType);
            }
            catch (Exception ex) {
                LogFacade.Instance.LogError("Could not determine or instantiate worker type from assembly " + assName, ex);
                throw;
            }
            LogFacade.Instance.LogInfo("Selected worker type " + workerType.Name + " from assembly " + assName);
            return workerType;
        }

    }
}

