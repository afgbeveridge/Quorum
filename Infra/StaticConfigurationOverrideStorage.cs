using System.Collections.Generic;

namespace Infra {

    public class StaticConfigurationOverrideStorage : IConfigurationOverrideStorage {

        private static readonly Dictionary<string, object> Overrides = new Dictionary<string, object>();

        public void Set(string key, object value) {
            Overrides[key] = value;
        }

        public object ValueOrDefault(string key, object defaultValue) {
            return Overrides.ContainsKey(key) ? Overrides[key] : defaultValue;
        }
    }
}
