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
    
    public class ConsoleLogger : AbstractLogger {

        public override ILogger Configure(LoggingOptions options = null) {
            return this;
        }

        public override ILogger Log(LogLevel level, string message, Exception ex = null) {
            Console.WriteLine(message);
            return this;
        }
    }

}
