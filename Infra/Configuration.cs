
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Infra {

    public class Configuration : IConfiguration {

        public T Get<T>(string key, T defaultValue = default(T)) {
            var setting = ConfigurationManager.AppSettings[key];
            return setting == null ? defaultValue : (T)Convert.ChangeType(setting, typeof(T));
        }

        public TValue Get<TCaller, TValue>(string key, TValue defaultValue = default(TValue)) {
            return Get<TValue>(typeof(TCaller).Name + "." + key, defaultValue);
        }

        public T Get<T>(ConfigurationItem<T> src) {
            var setting = ConfigurationManager.AppSettings[src.Key];
            return setting == null ? src.DefaultValue : (T)Convert.ChangeType(setting, typeof(T));
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
