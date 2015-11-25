using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Quorum.Integration.Http;
using Infra;

namespace QuorumWindowsService {

    public partial class QuorumAwareWindowsService : ServiceBase {

        private HttpQuorumImpl Adapter { get; set; }

        public QuorumAwareWindowsService() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            Adapter = new HttpQuorumImpl().Start<WorkerAdapter>();
        }

        protected override void OnStop() {
            if (Adapter.IsNotNull())
                Adapter.Stop().Wait();
        }
    }
}

