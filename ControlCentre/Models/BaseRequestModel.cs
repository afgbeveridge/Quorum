using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlCentre.Models {
    
    public class BaseRequestModel {
        public int Port { get; set; }
        public int Timeout { get; set; }
    }
}