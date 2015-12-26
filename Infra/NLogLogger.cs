#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
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

        private LoggingOptions LogOptions { get; set; }

        public override ILOGGER Configure(LoggingOptions options = null) {
            return this.Fluently(_ => {
                if (Configuration.IsNull()) {
                    LogOptions = options ?? new LoggingOptions();
                    Configuration = new LoggingConfiguration();
                    LogOptions.RequireFileSink.IfTrue(() => CreateFileTarget(DiagnosticLoggerName, "${basedir}/diagnostics.txt", DefaultLayout));
                    LogOptions.RequireEventLogSink.IfTrue(() => CreateEventLogTarget());
                    LogOptions.RequireConsoleSink.IfTrue(() => CreateConsoleTarget());
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

            LoggingRule rule = new LoggingRule(name, Mapping[LogOptions.MinimalLogLevel], fileTarget);
            Configuration.LoggingRules.Add(rule);
        }

        private void CreateEventLogTarget() {
            EventLogTarget target = new EventLogTarget {
                Source = "Quorum",
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
            Configuration.LoggingRules.Add(new LoggingRule("*", Mapping[LogOptions.MinimalLogLevel], target));
        }
    }
}
