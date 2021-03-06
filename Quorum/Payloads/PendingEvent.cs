﻿#region License
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

namespace Quorum.Payloads {
    
    public class PendingEvent {

        public Guid Id { get; set;}

        public string Name { get; set; }

        public DateTime CreatedOn { get; set; }

        public double AgeInSeconds { get; set; }

    }
}
