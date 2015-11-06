using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra {
    
    public class ConsoleLogger : AbstractLogger {

        public override ILogger Configure() {
            return this;
        }

        public override ILogger Log(LogLevel level, string message, Exception ex = null) {
            Console.WriteLine(message);
            return this;
        }
    }

}
