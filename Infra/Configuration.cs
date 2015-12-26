#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion

using System;
using System.Configuration;

namespace Infra {

    public class Configuration : IConfiguration {

        public IConfigurationOverrideStorage OverrideStorage { get; set; }

        public Configuration() : this(new StaticConfigurationOverrideStorage()) {
        }

        public Configuration(IConfigurationOverrideStorage overrideStorage) {
            OverrideStorage = overrideStorage;
        }

        public T Get<T>(string key, T defaultValue = default(T)) {
            var setting = OverrideStorage.ValueOrDefault(key, ConfigurationManager.AppSettings[key]);
            return setting == null ? defaultValue : (T)Convert.ChangeType(setting, typeof(T));
        }

        public TValue Get<TCaller, TValue>(string key, TValue defaultValue = default(TValue)) {
            return Get<TValue>(typeof(TCaller).Name + "." + key, defaultValue);
        }

        public T Get<T>(ConfigurationItem<T> src) {
            return Get(src.Key, src.DefaultValue);
        }

        public void LocalSet<T>(string key, T value) {
            OverrideStorage.Set(key, value);
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
