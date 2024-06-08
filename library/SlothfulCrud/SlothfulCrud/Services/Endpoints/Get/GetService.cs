using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Extensions;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Services.Endpoints.Get
{
    internal class GetService<TEntity, TContext> : BaseEndpointService<TEntity>, IGetService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private TContext DbContext { get; }
        
        public GetService(TContext dbContext, IEntityConfigurationProvider configurationProvider) : base(configurationProvider)
        {
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
                .FirstOrDefault(x => EF.Property<object>(x, EntityConfiguration.KeyProperty).Equals(id))
                .OrFail($"{typeof(TEntity)}NotFound", $"{typeof(TEntity)} with {EntityConfiguration.KeyProperty}: '{id}' not found.");
        }
    }
}