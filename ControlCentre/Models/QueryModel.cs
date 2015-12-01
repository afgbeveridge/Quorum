using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlCentre.Models {

    public class QueryModel : BaseRequestModel {
        public IEnumerable<string> Machines { get; set; }
    }

}