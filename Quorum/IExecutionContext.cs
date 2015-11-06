using System;
using System.Collections.Generic;
using Quorum.Payloads;
using Quorum.Integration;

namespace Quorum {

    public interface IExecutionContext {

        // Held in a container object now
        //object EventPayload { get; }
        
        IPayloadParser PayloadParser { get; set; }
        
        bool IsMaster { get; set; }

        Neighbourhood Network { get; set; }

        string LocalAddress { get; set; }

        string HostName { get; set; }

        IDictionary<Guid, object> StateStore { get; }
    }

}
