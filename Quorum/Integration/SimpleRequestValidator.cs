#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using Infra;
using System.Collections.Generic;

namespace Quorum.Integration {

    public class SimpleRequestValidator : IRequestValidator {

        public SimpleRequestValidator(INetworkEnvironment env) {
            Network = env;
        }

        private INetworkEnvironment Network { get; set; }

        public void Validate(IDictionary<string, string> directives, object outerContext) {
            ExamineFramingDirectives(directives);
            ExamineAuthentication(directives);
        }

        public IConfiguration Config { get; set; }

        protected virtual void ExamineFramingDirectives(IDictionary<string, string> directives) {
            var header = Config.Get(Constants.Configuration.CustomHeader);
            if (Config.Get(Constants.Configuration.EmitCustomHeader) && header.IsNotNull()) {
                DBC.True(directives.ContainsKey(header), () => "Expected request to contain header " + header);
                // TODO: Check value matches the determinable quorum id of the sender
                LogFacade.Instance.LogDebug("Inbound request qualification: (" + header + ", " + directives[header] + ")");
            }
        }

        protected virtual void ExamineAuthentication(IDictionary<string, string> directives) {

        }

    }
}
