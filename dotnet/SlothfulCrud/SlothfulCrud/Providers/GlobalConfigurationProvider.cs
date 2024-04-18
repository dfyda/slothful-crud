using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Providers
{
    public class GlobalConfigurationProvider : IGlobalConfigurationProvider
    {
        private readonly Dictionary<string, GlobalConfiguration> _configurations = new Dictionary<string, GlobalConfiguration>();
        
        public void Register(Type type, GlobalConfiguration configuration)
        {
            _configurations.TryAdd(type.Name, configuration);
        }

        public GlobalConfiguration GetConfiguration(Type type)
        {
            return _configurations.TryGetValue(type.Name, out var configuration) ? configuration : new GlobalConfiguration();
        }
    }   
}