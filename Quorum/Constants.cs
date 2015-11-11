using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infra;

namespace Quorum {
    
    public static class Constants {

        public static class Configuration {

            public static readonly ConfigurationItem<string> Nexus = ConfigurationItem<string>.Create("quorum.environment");
            public static readonly ConfigurationItem<int> HttpListenerPort = ConfigurationItem<int>.Create("quorum.http.listenerPort", 9999);
            public static readonly ConfigurationItem<string> MachineStrength = ConfigurationItem<string>.Create("quorum.machine.strength");
            public static readonly ConfigurationItem<int> DiscoveryPeriodMs = ConfigurationItem<int>.Create("quorum.discoveryPeriodMs", 10000);
            public static readonly ConfigurationItem<int> ResponseLimit = ConfigurationItem<int>.Create("quorum.http.responseLimit", 5000);

        }

        internal static class Local {

            internal static readonly Guid MasterStateTaskKey = Guid.Empty;

        }

    }

}
