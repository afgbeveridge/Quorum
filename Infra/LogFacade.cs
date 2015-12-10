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
    
    public class LogFacade : AbstractLogger {

        private static readonly Lazy<LogFacade> Singleton = new Lazy<LogFacade>(() => new LogFacade());

        private LogFacade() { }

        public static LogFacade Instance { get { return Singleton.Value; } }

        public ILogger Adapter { get; set; }

        public override ILogger Configure(LoggingOptions options = null) {
            return this;
        }

        public override ILogger Log(LogLevel level, string message, Exception ex = null) {
            if (Adapter.IsNotNull())
                Adapter.Log(level, message);
            return this;
        }
    }
}
