using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Quorum.Integration;
using Quorum;
using Infra;
using System.Reflection;

namespace QuorumWindowsService {

    public enum Effect { Start, Stop }

    public partial class QuorumAwareWindowsService : ServiceBase {

        private QuorumImplFacade Adapter { get; set; }

        public QuorumAwareWindowsService() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            // Do this early to log any assembly load issues
            QuorumImplFacade.ConfigureLogging();
            Adapter = new QuorumImplFacade()
                        .WithBuilder(new Builder())
                        .Start(GetWorkerImplementation());
        }

        protected override void OnStop() {
            if (Adapter.IsNotNull())
                Adapter.Stop().Wait();
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
                Assert.False(assName.IsNull(), () => "You must specify an assembly name in quorum.workerTypeAssembly");
                Assembly assembly = Assembly.Load(assName);
                workerType = assembly.GetExportedTypes().FirstOrDefault(t => t.GetInterfaces().Contains(typeof(IMasterWorkAdapter)) && !t.IsAbstract);
                Assert.False(workerType.IsNull(), () => "No type in " + assName + " implements the worker interface");
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

