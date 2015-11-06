using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quorum.Payloads {
    
    public class Neighbourhood {

        public Neighbourhood() {
            Neighbours = new List<Neighbour>();
        }

        public List<Neighbour> Neighbours { get; set; }

        public DateTime LastChecked { get; set; }

    }

}
