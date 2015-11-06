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
