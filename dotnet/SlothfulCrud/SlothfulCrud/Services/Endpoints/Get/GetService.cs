using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Extensions;
using SlothfulCrud.Providers;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Services.Endpoints.Get
{
    public class GetService<T, TContext> : IGetService<T, TContext> 
        where T : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly EntityConfiguration _entityConfiguration;
        private TContext DbContext { get; }
        
        public GetService(TContext dbContext, IEntityConfigurationProvider configurationProvider)
        {
            _entityConfiguration = configurationProvider.GetConfiguration(typeof(T));
            DbContext = dbContext;
        }
        
        // TODO: Key property should be from EntityConfiguration
        // Refactor other places to use KeyProperty from EntityConfiguration
        public T Get(Guid id)
        {
            CheckEntityKey(typeof(T));

            return GetEntity(id);
        }

        private T GetEntity<TField>(TField id)
        {
            return DbContext.Set<T>()
                .IncludeAllFirstLevelDependencies()
                .FirstOrDefault(x => EF.Property<TField>(x, _entityConfiguration.KeyProperty).Equals(id))
                .OrFail($"{typeof(T)}NotFound", $"{typeof(T)} with {_entityConfiguration.KeyProperty}: '{id}' not found.");
        }

        private void CheckEntityKey(Type type)
        {
            if (type.GetProperty(_entityConfiguration.KeyProperty) is null)
            {
                throw new ConfigurationException($"Entity '{typeof(T)}' must have a property named '{_entityConfiguration.KeyProperty}'");
            };
        }
    }
}