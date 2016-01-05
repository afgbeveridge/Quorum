using System;

namespace Infra {

    public class CachedLogEntry {
        public DateTime When { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
    }
}
