using Infra;

namespace Quorum {

    public static class IConfigurationExtensions {

        public static IConfiguration WithAppropriateOverrides(this IConfiguration cfg) {
            cfg.OverrideStorage = ActiveDisposition.Shared ? new TransientConfigurationOverrideStorage() : (IConfigurationOverrideStorage) new StaticConfigurationOverrideStorage();
            return cfg;
        }

    }
}
