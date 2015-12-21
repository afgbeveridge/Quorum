#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using Infra;
using System.Collections.Generic;

namespace Quorum.Integration {

    public class SimpleRequestValidator : IRequestValidator {

        public SimpleRequestValidator(INetworkEnvironment env) {
            Network = env;
        }

        private INetworkEnvironment Network { get; set; }

        public void ValidateRequestSize(long size) {
            var max = Config.Get(Constants.Configuration.MaxPayloadLength);
            LogFacade.Instance.LogDebug("Inbound request size validation: (" + size + ", " + max + ")");
            DBC.True(size <= max, () => "Query payload must be rejected, size exceeds maximum configured (" + size + "," + max + ")");
        }

        public void ValidateDirectives(IDictionary<string, string> directives, object outerContext) {
            ExamineFramingDirectives(directives);
            ExamineAuthentication(directives);
        }

        public IConfiguration Config { get; set; }

        protected virtual void ExamineFramingDirectives(IDictionary<string, string> directives) {
            var header = Config.Get(Constants.Configuration.CustomHeader);
            if (Config.Get(Constants.Configuration.EmitCustomHeader) && header.IsNotNull()) {
                Action<string> checkHeader = name => DBC.True(directives.ContainsKey(name), () => "Expected request to contain header " + name);
                checkHeader(header);
                LogFacade.Instance.LogDebug("Inbound request qualification: (" + header + ", " + directives[header] + ")");
                var seed = Config.Get(Constants.Configuration.HostNameHeader);
                checkHeader(seed);
                var derived = Network.DeriveUniqueId(directives[seed]);
                LogFacade.Instance.LogDebug("Seed derived: " + derived);
                DBC.True(long.Parse(directives[header]) == derived, () => "Node Id mismatch: (" + directives[header] + "," + directives[seed] + ")");
            }
        }

        protected virtual void ExamineAuthentication(IDictionary<string, string> directives) {

        }

    }
}
