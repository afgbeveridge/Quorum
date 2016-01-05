#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace Infra {

    public class LogFacade : AbstractLogger {

        private const int DefaultCacheSize = 100;
        private const int MaximumCacheSize = 500;
        private int ConfiguredCacheSize = DefaultCacheSize;
        private static readonly Lazy<LogFacade> Singleton = new Lazy<LogFacade>(() => new LogFacade());
        private readonly Lazy<List<CachedLogEntry>> MostRecentEvents = new Lazy<List<CachedLogEntry>>(() => new List<CachedLogEntry>(DefaultCacheSize));
        private readonly ReaderWriterLockSlim CacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private Dictionary<Guid, Action<object>> Listeners { get; set; }

        private LogFacade() {
            Listeners = new Dictionary<Guid, Action<object>>();
        }

        public void AddListener(Guid key, Action<object> handler) {
            Listeners[key] = handler;
        }

        public void RemoveListener(Guid key) {
            if (Listeners.ContainsKey(key))
                Listeners.Remove(key);
        }

        public static LogFacade Instance { get { return Singleton.Value; } }

        public ILogger Adapter { get; set; }

        public override ILogger Configure(LoggingOptions options = null) {
            return this;
        }

        public override ILogger Log(LogLevel level, string message, Exception ex = null) {
            if (Adapter.IsNotNull()) {
                Adapter.Log(level, message);
                CacheEvent(message, level);
            }
            return this;
        }

        public int NumberOfCachedEvents {
            get {
                return MostRecentEvents.Value.Count;
            }
        }

        private void LockAndAct(Action f) {
            try {
                CacheLock.EnterWriteLock();
                f();
            }
            finally {
                CacheLock.ExitWriteLock();
            }
        }

        private void CacheEvent(string msg, LogLevel level) {
            CachedLogEntry entry = new CachedLogEntry {
                When = DateTime.Now,
                Message = msg,
                Level = level
            };
            LockAndAct(() => {
                FixedCache.Insert(0, entry);
                if (FixedCache.Count > ConfiguredCacheSize)
                    FixedCache.RemoveAt(ConfiguredCacheSize);
            });
            Broadcast(entry);
        }

        private void Broadcast(CachedLogEntry entry) {
            if (Listeners.Any())
                Listeners.ToList().ForEach(kvp => kvp.Value(entry));
        }

        private List<CachedLogEntry> FixedCache { get { return MostRecentEvents.Value; } }

        public IEnumerable<CachedLogEntry> Entries(int start, int howMany) {
            IEnumerable<CachedLogEntry> result = null;
            LockAndAct(() => FixedCache.Skip(start).Take(howMany).ToArray());
            return result;
        }

        public int CacheSize {
            get {
                return ConfiguredCacheSize;
            }
            set {
                if (value != ConfiguredCacheSize) {
                    LockAndAct(() => {
                        ConfiguredCacheSize = Math.Min(Math.Max(DefaultCacheSize, value), MaximumCacheSize);
                        if (ConfiguredCacheSize < FixedCache.Count)
                            FixedCache.RemoveRange(ConfiguredCacheSize, FixedCache.Count - ConfiguredCacheSize);
                    });
                }
            }
        }
    }
}
