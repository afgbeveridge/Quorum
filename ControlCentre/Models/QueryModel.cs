#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System.Collections.Generic;

namespace ControlCentre.Models {

    public class QueryModel : BaseRequestModel {
        public IEnumerable<string> Machines { get; set; }
    }

}