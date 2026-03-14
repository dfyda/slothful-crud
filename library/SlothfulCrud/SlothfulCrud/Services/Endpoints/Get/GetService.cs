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
        
        public async Task<TEntity> GetAsync(object keyProperty)
        {
            CheckEntityKey(typeof(TEntity), keyProperty);

            return await GetEntityAsync(keyProperty);
        }

        private async Task<TEntity> GetEntityAsync(object id)
        {
            var entity = await DbContext.Set<TEntity>()
                .IncludeAllFirstLevelDependencies()
                .FirstOrDefaultAsync(x => EF.Property<object>(x, EntityConfiguration.KeyProperty).Equals(id));
            
            return entity
                .OrFail($"{typeof(TEntity)}NotFound", $"{typeof(TEntity)} with {EntityConfiguration.KeyProperty}: '{id}' not found.");
        }
    }
}
