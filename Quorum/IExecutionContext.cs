#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
using Quorum.Payloads;
using Quorum.Integration;
using FSM;

namespace Quorum {

    public interface IExecutionContext : IMinimalContext {

        // Held in a container object now
        //object EventPayload { get; }
        
        IPayloadParser PayloadParser { get; set; }
        
        bool IsMaster { get; set; }

        Neighbourhood Network { get; set; }

        string LocalAddress { get; set; }

        IDictionary<Guid, object> StateStore { get; }

        bool InEligibleForElection { get; set; }

        long WorkerExecutionUnits { get; set; }
    }

}
