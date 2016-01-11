using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlCentre.Models {

    public class SelfDiscoveryRequest : QueryModel {
        public IEnumerable<string> PossibleNeighbours { get; set; }
    }
}