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
        protected readonly SlothfulOptions Options;

        protected BaseEndpointService(
            IEntityConfigurationProvider configurationProvider,
            SlothfulOptions options)
        {
            EntityConfiguration = configurationProvider.GetConfiguration(typeof(TEntity));
            Options = options;
        }
        
        protected IQueryable<T> ApplyQueryCustomizer<T>(IQueryable<T> query)
        {
            if (Options?.QueryCustomizer is not null)
            {
                var customized = Options.QueryCustomizer(query);
                if (customized is not IQueryable<T> typed)
                {
                    throw new InvalidOperationException(
                        $"QueryCustomizer must return IQueryable<{typeof(T).Name}>, but returned {customized?.GetType().Name ?? "null"}.");
                }
                return typed;
            }
            return query;
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
            
            if (ReflectionCache.GetProperty(type, EntityConfiguration.KeyProperty) is null)
            {
                throw new ConfigurationException($"Entity '{typeof(TEntity)}' must have a property named '{EntityConfiguration.KeyProperty}'");
            }

            if (keyProperty.GetType() != EntityConfiguration.KeyPropertyType)
            {
                throw new ConfigurationException($"Entity '{typeof(TEntity)}' key property '{EntityConfiguration.KeyProperty}' must be of type '{EntityConfiguration.KeyPropertyType}'");
            }
        }
    }
}
