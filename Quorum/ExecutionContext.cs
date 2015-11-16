using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quorum.Payloads;
using Quorum.Integration;
using Infra;

namespace Quorum {
    
    public class ExecutionContext : IExecutionContext {

        public ExecutionContext(INetworkEnvironment env) {
            Network = new Neighbourhood();
            StateStore = new ConcurrentDictionary<Guid, object>();
            LocalAddress = env.LocalIPAddress.ToString();
            HostName = env.HostName;
        }

        /// <summary>
        /// This is weakly typed, presumably to be transformed to a more useful form
        /// </summary>
        public object EventPayload { get; internal set; }

        public IPayloadParser PayloadParser { get; set; }

        public bool IsMaster { get; set; }

        public bool InEligibleForElection { get; set; }

        public Neighbourhood Network { get; set; }

        public string LocalAddress { get; set; }

        public string HostName { get; set; }

        public long WorkerExecutionUnits { get; set; }

        public IDictionary<Guid, object> StateStore { get; private set; }

    }

}
