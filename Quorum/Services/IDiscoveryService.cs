using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quorum.Payloads;

namespace Quorum.Services {
    
    public interface IDiscoveryService {

        IEnumerable<Neighbour> Discover(string invokingHostName);

    }
}
