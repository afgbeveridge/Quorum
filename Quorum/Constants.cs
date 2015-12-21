#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using Infra;

namespace Quorum {

    public static class Constants {

        public static class Configuration {

            public static readonly ConfigurationItem<string> Nexus = ConfigurationItem<string>.Create("quorum.environment");
            public static readonly ConfigurationItem<int> ExternalEventListenerPort = ConfigurationItem<int>.Create("quorum.listenerPort", 9999);
            public static readonly ConfigurationItem<string> MachineStrength = ConfigurationItem<string>.Create("quorum.machine.strength");
            public static readonly ConfigurationItem<int> DiscoveryPeriodMs = ConfigurationItem<int>.Create("quorum.discoveryPeriodMs", 30000);
            public static readonly ConfigurationItem<int> ResponseLimit = ConfigurationItem<int>.Create("quorum.responseLimit", 5000);
            public static readonly ConfigurationItem<int> TcpBacklogSize = ConfigurationItem<int>.Create("quorum.tcp.backlogSize", 5);
            public static readonly ConfigurationItem<int> TcpFrameSizeSpecificationLength = ConfigurationItem<int>.Create("quorum.tcp.frameSizeSpecificationLength", 6);
            public static readonly ConfigurationItem<int> TcpConnectionTimeout = ConfigurationItem<int>.Create("quorum.tcp.connectionTimeout", 1000);
            public static readonly ConfigurationItem<string> DefaultTransport = ConfigurationItem<string>.Create("quorum.transport", "http");
            // Info, Warning, Error, Exception, Debug
            public static readonly ConfigurationItem<string> MinimalLogLevel = ConfigurationItem<string>.Create("quorum.minimalLogLevel", "Debug");
            public static readonly ConfigurationItem<bool> EncryptedTransportRequired = ConfigurationItem<bool>.Create("quorum.encryptedTransportRequired", false);
            public static readonly ConfigurationItem<string> CustomHeader = ConfigurationItem<string>.Create("quorum.customHeader", "X-QuorumMember");
            public static readonly ConfigurationItem<bool> EmitCustomHeader = ConfigurationItem<bool>.Create("quorum.emitCustomHeader", true);

        }

        public static class Local {

            public static readonly Guid MasterStateTaskKey = Guid.Empty;

        }

    }

}
