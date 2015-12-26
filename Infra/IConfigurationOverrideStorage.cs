namespace Infra {

    public interface IConfigurationOverrideStorage {

        object ValueOrDefault(string key, object defaultValue);

        void Set(string key, object value);

    }
}
