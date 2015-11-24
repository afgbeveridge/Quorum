
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Infra {

    public class Configuration : IConfiguration {

        private static readonly Dictionary<string, object> LocalOverrides = new Dictionary<string, object>();

        public T Get<T>(string key, T defaultValue = default(T)) {
            var setting = LocalOverrides.ContainsKey(key) ? LocalOverrides[key] : ConfigurationManager.AppSettings[key];
            return setting == null ? defaultValue : (T)Convert.ChangeType(setting, typeof(T));
        }

        public TValue Get<TCaller, TValue>(string key, TValue defaultValue = default(TValue)) {
            return Get<TValue>(typeof(TCaller).Name + "." + key, defaultValue);
        }

        public T Get<T>(ConfigurationItem<T> src) {
            return Get(src.Key, src.DefaultValue);
        }

        public void LocalSet<T>(string key, T value) {
            LocalOverrides[key] = value;
        }

    }

    public class ConfigurationItem<TType> {
        public string Key { get; set; }
        public TType DefaultValue { get; set; }
        public static ConfigurationItem<TType> Create(string key, TType defaultValue = default(TType)) {
            return new ConfigurationItem<TType> { Key = key, DefaultValue = defaultValue };
        }
    }

}
