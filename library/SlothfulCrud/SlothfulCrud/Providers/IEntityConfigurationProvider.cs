using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Providers
{
    internal interface IEntityConfigurationProvider
    {
        void Register(Type type, EntityConfiguration configuration);
        EntityConfiguration GetConfiguration(Type type);
    }   
}