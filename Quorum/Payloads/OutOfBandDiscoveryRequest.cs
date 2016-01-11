using System.Collections.Generic;

namespace Quorum.Payloads {

    public class OutOfBandDiscoveryRequest : BasePayload {

        public IEnumerable<string> MootedNeighbours { get; set; } 

    }
}
