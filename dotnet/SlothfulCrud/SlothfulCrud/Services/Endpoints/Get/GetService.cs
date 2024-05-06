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
        
        // TODO: Map to concrete type in dtos, query and commands.
        public TEntity Get(object id)
        {
            CheckEntityKey(typeof(TEntity));

            return GetEntity(id);
        }

        private TEntity GetEntity(object id)
        {
            return DbContext.Set<TEntity>()
                .IncludeAllFirstLevelDependencies()
                .FirstOrDefault(x => EF.Property<object>(x, _entityConfiguration.KeyProperty).Equals(id))
                .OrFail($"{typeof(TEntity)}NotFound", $"{typeof(TEntity)} with {_entityConfiguration.KeyProperty}: '{id}' not found.");
        }

        private void CheckEntityKey(Type type)
        {
            if (type.GetProperty(_entityConfiguration.KeyProperty) is null)
            {
                throw new ConfigurationException($"Entity '{typeof(TEntity)}' must have a property named '{_entityConfiguration.KeyProperty}'");
            };
        }
    }
}