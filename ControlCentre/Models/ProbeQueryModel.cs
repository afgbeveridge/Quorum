#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlCentre.Models {

    public class ProbeQueryModel : BaseRequestModel {
        public string Scope { get; set; }
    }
}