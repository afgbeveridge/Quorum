using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra {
    
    public static class LogFacade {

        public static ILogger Adapter { get; set; }

        public static void Log(string msg, LogLevel level = LogLevel.Debug) {
            if (Adapter.IsNotNull())
                Adapter.Log(level, msg);
        }

    }
}
