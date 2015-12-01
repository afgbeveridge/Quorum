using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlCentre.Models {

    public class TargetRequestModel : BaseRequestModel {
        public string Name { get; set; }
        public bool RenderIneligible { get; set; }
        public bool RenderEligible { get; set; }
    }
}