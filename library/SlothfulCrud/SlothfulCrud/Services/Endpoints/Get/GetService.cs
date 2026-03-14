using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Extensions;
using SlothfulCrud.Providers;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Services.Endpoints.Get
{
    internal class GetService<TEntity, TContext> : BaseEndpointService<TEntity>, IGetService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private TContext DbContext { get; }
        
        public GetService(
            TContext dbContext,
            IEntityConfigurationProvider configurationProvider,
            SlothfulOptions options) : base(configurationProvider, options)
        {
            DbContext = dbContext;
        }
        
        public async Task<TEntity> GetAsync(object keyProperty)
        {
            CheckEntityKey(typeof(TEntity), keyProperty);

            return await GetEntityAsync(keyProperty);
        }

        private async Task<TEntity> GetEntityAsync(object id)
        {
            var query = DbContext.Set<TEntity>()
                .IncludeAllFirstLevelDependencies();
            query = ApplyQueryCustomizer(query);

            var entity = await query
                .FirstOrDefaultAsync(x => EF.Property<object>(x, EntityConfiguration.KeyProperty).Equals(id));
            
            return entity
                .OrFail($"{typeof(TEntity)}NotFound", $"{typeof(TEntity)} with {EntityConfiguration.KeyProperty}: '{id}' not found.");
        }
    }
}
