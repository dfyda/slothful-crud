using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Providers
{
    public class EntityConfigurationProvider : IEntityConfigurationProvider
    {
        private readonly Dictionary<string, EntityConfiguration> _configurations = new Dictionary<string, EntityConfiguration>();
        
        public void Register(Type type, EntityConfiguration configuration)
        {
            _configurations.TryAdd(type.Name, configuration);
        }

        public EntityConfiguration GetConfiguration(Type type)
        {
            return _configurations.TryGetValue(type.Name, out var configuration) ? configuration : new EntityConfiguration();
        }
    }   
}