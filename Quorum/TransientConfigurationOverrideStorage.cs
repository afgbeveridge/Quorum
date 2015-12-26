using System.Web;
using Infra;

namespace Quorum {

    public class TransientConfigurationOverrideStorage : IConfigurationOverrideStorage {

        public void Set(string key, object value) {
            HttpContext.Current.Items[key] = value;
        }

        public object ValueOrDefault(string key, object defaultValue) {
            return HttpContext.Current.Items[key] ?? defaultValue;
        }

    }
}
