using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Providers
{
    public interface IGlobalConfigurationProvider
    {
        void Register(Type type, GlobalConfiguration configuration);
        GlobalConfiguration GetConfiguration(Type type);
    }   
}