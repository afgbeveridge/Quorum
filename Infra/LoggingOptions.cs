#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using Infra;

namespace Infra {

    public class LoggingOptions {

        public LoggingOptions() {
            RequireConsoleSink = RequireEventLogSink = RequireFileSink = true;
            MinimalLogLevel = LogLevel.Debug;
        }

        public bool RequireFileSink { get; set; }

        public bool RequireEventLogSink { get; set; }

        public bool RequireConsoleSink { get; set; }

        public LogLevel MinimalLogLevel { get; set; }

    }
}
