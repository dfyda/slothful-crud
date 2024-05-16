using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Extensions;
using SlothfulCrud.Providers;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Services.Endpoints.Get
{
    public class GetService<TEntity, TContext> : IGetService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly EntityConfiguration _entityConfiguration;
        private TContext DbContext { get; }
        
        public GetService(TContext dbContext, IEntityConfigurationProvider configurationProvider)
        {
            _entityConfiguration = configurationProvider.GetConfiguration(typeof(TEntity));
            DbContext = dbContext;
        }
        
        public TEntity Get(object keyProperty)
        {
            CheckEntityKey(typeof(TEntity), keyProperty);

            return GetEntity(keyProperty);
        }

        private TEntity GetEntity(object id)
        {
            return DbContext.Set<TEntity>()
                .IncludeAllFirstLevelDependencies()
                .FirstOrDefault(x => EF.Property<object>(x, _entityConfiguration.KeyProperty).Equals(id))
                .OrFail($"{typeof(TEntity)}NotFound", $"{typeof(TEntity)} with {_entityConfiguration.KeyProperty}: '{id}' not found.");
        }

        private void CheckEntityKey(Type type, object keyProperty)
        {
            if (_entityConfiguration is null)
            {
                throw new ConfigurationException($"Entity '{typeof(TEntity)}' has no configuration.");
            }

            if (keyProperty is null)
            {
                throw new ConfigurationException($"Parameter '{nameof(keyProperty)}' cannot be null.");
            }
            
            if (type.GetProperty(_entityConfiguration.KeyProperty) is null)
            {
                throw new ConfigurationException($"Entity '{typeof(TEntity)}' must have a property named '{_entityConfiguration.KeyProperty}'");
            };

            if (keyProperty.GetType() != _entityConfiguration.KeyPropertyType)
            {
                throw new ConfigurationException($"Entity '{typeof(TEntity)}' key property '{_entityConfiguration.KeyProperty}' must be of type '{_entityConfiguration.KeyPropertyType}'");
            }
        }
    }
}