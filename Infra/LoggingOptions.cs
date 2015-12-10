#region License
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

namespace Infra {

    public class LoggingOptions {

        public LoggingOptions() {
            RequireConsoleSink = RequireEventLogSink = RequireFileSink = true;
        }

        public bool RequireFileSink { get; set; }

        public bool RequireEventLogSink { get; set; }

        public bool RequireConsoleSink { get; set; }

    }
}
