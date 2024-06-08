using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Providers
{
    internal class EntityPropertyKeyValueProvider<TEntity> : IEntityPropertyKeyValueProvider<TEntity>
        where TEntity : class, ISlothfulEntity
    {
        public object GetNextValue(EntityConfiguration configuration)
        {
            if (configuration.KeyPropertyType == typeof(Guid))
            {
                return Guid.NewGuid();
            }

            throw new ConfigurationException(
                $"Please implement IEntityPropertyKeyValueProvider for '{configuration.KeyPropertyType}' type. By default, only Guid type is supported.");
        }
    }
}