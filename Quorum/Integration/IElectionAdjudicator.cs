#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quorum.Payloads;

namespace Quorum.Integration {
    
    public interface IElectionAdjudicator {

        Neighbour Choose(IList<Neighbour> neighbours, Neighbour self);

    }
}
