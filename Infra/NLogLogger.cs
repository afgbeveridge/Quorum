#region License
/*
Copyright (c) 2015 Tony Beveridge

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without 
restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to 
whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE 
AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;
using ILOGGER = Infra.ILogger;
using LEVEL = Infra.LogLevel;

namespace Infra {

    /// <summary>
    /// An ILogger implementation that wraps NLog
    /// </summary>
    public class NLogLogger : AbstractLogger {

        private static readonly Dictionary<LEVEL, NLog.LogLevel> Mapping = new Dictionary<LEVEL, NLog.LogLevel> { 
            { LEVEL.Debug, NLog.LogLevel.Debug },
            { LEVEL.Info, NLog.LogLevel.Info },
            { LEVEL.Warning, NLog.LogLevel.Warn },
            { LEVEL.Error, NLog.LogLevel.Error },
            { LEVEL.Exception, NLog.LogLevel.Fatal }
        };

        public override ILOGGER Log(LEVEL level, string message, Exception ex = null) {
            return this.Fluently(_ => {
                var type = Mapping[level];
                ex
                    .IsNull()
                    .IfTrue(() => Diagnostics.Log(type, message))
                    .IfFalse(() => Diagnostics.Log(type, ex, message));
                ;
            });
        }

        private const string DefaultLayout = @"${date:format=yyyy-MM-dd HH\:mm\:ss.fff} ${logger} ${message}${newline}${exception:format=ToString,StackTrace}";
        private const string DiagnosticLoggerName = "Diagnostics";

        public override ILOGGER Configure() {
            return this.Fluently(_ => {
                if (Configuration.IsNull()) {
                    Configuration = new LoggingConfiguration();
                    CreateFileTarget(DiagnosticLoggerName, "${basedir}/diagnostics.txt", DefaultLayout);
                    CreateEventLogTarget();
                    CreateConsoleTarget();
                    LogManager.Configuration = Configuration;
                    Diagnostics.Info("Default NLog log configuration established");
                }
            });
        }

        protected LoggingConfiguration Configuration { get; set; }

        protected virtual Logger Diagnostics {
            get {
                return LogManager.GetLogger(DiagnosticLoggerName);
            }
        }

        private void CreateFileTarget(string name, string fileName, string layout) {

            FileTarget fileTarget = new FileTarget {
                FileName = fileName,
                Layout = layout,
                ArchiveAboveSize = 10000000L
            };

            Configuration.AddTarget("file", fileTarget);

            LoggingRule rule = new LoggingRule(name, NLog.LogLevel.Debug, fileTarget);
            Configuration.LoggingRules.Add(rule);
        }

        private void CreateEventLogTarget() {
            EventLogTarget target = new EventLogTarget {
                Source = "NHooked",
                Log = "Application",
                MachineName = ".",
                Layout = DefaultLayout
            };
            Configuration.AddTarget("eventLog", target);
            Configuration.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Warn, target));
        }

        private void CreateConsoleTarget() {
            ColoredConsoleTarget target = new ColoredConsoleTarget {
                Layout = DefaultLayout
            }; 
            Configuration.AddTarget("console", target);
            Configuration.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Debug, target));
        }
    }
}
