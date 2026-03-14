using System.Collections.Concurrent;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Providers
{
    internal class EntityConfigurationProvider : IEntityConfigurationProvider
    {
        private static readonly EntityConfiguration DefaultConfiguration = new();
        private readonly ConcurrentDictionary<string, EntityConfiguration> _configurations = new();
        
        public void Register(Type type, EntityConfiguration configuration)
        {
            _configurations.TryAdd(type.Name, configuration);
        }

        public EntityConfiguration GetConfiguration(Type type)
        {
            return _configurations.TryGetValue(type.Name, out var configuration) ? configuration : DefaultConfiguration;
        }
    }   
}
