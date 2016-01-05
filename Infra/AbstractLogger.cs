#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;

namespace Infra {

    public abstract class AbstractLogger : ILogger {

        public abstract ILogger Configure(LoggingOptions options = null);

        public ILogger LogDebug(string message) {
            return Log(LogLevel.Debug, message);
        }

        public ILogger LogInfo(string message) {
            return Log(LogLevel.Info, message);
        }

        public ILogger LogWarning(string message, Exception ex = null) {
            return Log(LogLevel.Warning, message, ex);
        }

        public ILogger LogError(string message, Exception ex = null) {
            return Log(LogLevel.Error, message, ex);
        }

        public ILogger LogException(string message, Exception ex) {
            return Log(LogLevel.Exception, message, ex);
        }

        public abstract ILogger Log(LogLevel level, string message, Exception ex = null);

    }
}
