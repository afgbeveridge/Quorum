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

    public enum LogLevel { Info, Warning, Error, Exception, Debug }

    public interface ILogger {
        ILogger Configure(LoggingOptions options = null);
        ILogger LogInfo(string message);
        ILogger LogWarning(string message, Exception ex = null);
        ILogger LogError(string message, Exception ex = null);
        ILogger LogException(string message, Exception ex);
        ILogger Log(LogLevel level, string message, Exception ex = null);
    }
}
