using System.Collections.Generic;

namespace Quorum.Payloads {

    public class BroadcastDiscoveryResult : BasePayload {

        public IEnumerable<DiscoveryEncapsulate> QueriedMembers { get; set; }

    }

    public class DiscoveryEncapsulate {

        public string Name { get; set; }

        public IEnumerable<SingleDiscoveryResult> Results { get; set; }

    }

    public class SingleDiscoveryResult {

        public string Name { get; set; }

        public bool Contacted { get; set; }
    }

}
