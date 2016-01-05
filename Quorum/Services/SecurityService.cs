using Infra;
using Quorum.Integration.Tcp;
using System;
using System.Collections.Generic;

namespace Quorum.Services {

    public class SecurityService : ISecurityService {

        public INetworkEnvironment Network { get; set; }

        public IConfiguration Configuration { get; set; }

        public string EncodedFrameDirectivesFor(string host) {
            return new TcpBoundedFrame().SizeUpString(string.Empty, GetDirectives(Network.DeriveUniqueId(host), host));
        }

        public string GetRawFrameDirectives() {
            return GetDirectives(Network.UniqueId, Network.SeedForUniqueId);
        }

        public string DissectAndValidateFrame(string src) {
            // TODO: Validate size
            var frame = TcpBoundedFrame.DissectCompleteFrame(src);
            TcpBundle bundle = TcpBoundedFrame.Parse(frame);
            ValidateDirectives(bundle.Directives);
            return frame;
        }

        public void ValidateRequestSize(long size) {
            var max = Configuration.Get(Constants.Configuration.MaxPayloadLength);
            LogFacade.Instance.LogDebug("Inbound request size validation: (" + size + ", " + max + ")");
            DBC.True(size <= max, () => "Query payload must be rejected, size exceeds maximum configured (" + size + "," + max + ")");
        }

        public void ValidateDirectives(IDictionary<string, string> directives, object outerContext = null) {
            ExamineFramingDirectives(directives);
            ExamineAuthentication(directives);
        }

        protected virtual void ExamineFramingDirectives(IDictionary<string, string> directives) {
            var header = Configuration.Get(Constants.Configuration.CustomHeader);
            if (Configuration.Get(Constants.Configuration.EmitCustomHeader) && header.IsNotNull()) {
                // Some proxies or tools mangle custom header names
                Action<string> checkHeader = name => DBC.True(directives.ContainsKey(name), () => "Expected request to contain header " + name);
                header = header.ToLowerInvariant();
                checkHeader(header);
                LogFacade.Instance.LogDebug("Inbound request qualification: (" + header + ", " + directives[header] + ")");
                var seed = Configuration.Get(Constants.Configuration.HostNameHeader).ToLowerInvariant();
                checkHeader(seed);
                var derived = Network.DeriveUniqueId(directives[seed]);
                LogFacade.Instance.LogDebug("Seed derived: " + derived);
                DBC.True(long.Parse(directives[header]) == derived, () => "Node Id mismatch: (" + directives[header] + "," + directives[seed] + ")");
            }
        }

        protected virtual void ExamineAuthentication(IDictionary<string, string> directives) {

        }

        private string GetDirectives(long uniqueId, string seed) {
            string result = string.Empty;
            if (Configuration.Get(Constants.Configuration.EmitCustomHeader)) {
                result = TcpBoundedFrame.CombineDirectives(
                                TcpBoundedFrame.FormDirective(Configuration.Get(Constants.Configuration.CustomHeader), uniqueId),
                                TcpBoundedFrame.FormDirective(Configuration.Get(Constants.Configuration.HostNameHeader), seed)
                         );
            }
            return result;
        }
    }
}
