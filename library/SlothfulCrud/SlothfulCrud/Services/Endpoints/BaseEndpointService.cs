using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Providers;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Services.Endpoints
{
    internal abstract class BaseEndpointService<TEntity>
        where TEntity : class, ISlothfulEntity, new()
    {
        protected readonly EntityConfiguration EntityConfiguration;

        protected BaseEndpointService(
            IEntityConfigurationProvider configurationProvider)
        {
            EntityConfiguration = configurationProvider.GetConfiguration(typeof(TEntity));
        }
        
        protected void CheckEntityKey(Type type, object keyProperty)
        {
            if (EntityConfiguration is null)
            {
                throw new ConfigurationException($"Entity '{typeof(TEntity)}' has no configuration.");
            }

            if (keyProperty is null)
            {
                throw new ConfigurationException($"Parameter '{nameof(keyProperty)}' cannot be null.");
            }
            
            if (type.GetProperty(EntityConfiguration.KeyProperty) is null)
            {
                throw new ConfigurationException($"Entity '{typeof(TEntity)}' must have a property named '{EntityConfiguration.KeyProperty}'");
            };

            if (keyProperty.GetType() != EntityConfiguration.KeyPropertyType)
            {
                throw new ConfigurationException($"Entity '{typeof(TEntity)}' key property '{EntityConfiguration.KeyProperty}' must be of type '{EntityConfiguration.KeyPropertyType}'");
            }
        }
    }
}