using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Providers;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Services.Endpoints.Delete
{
    internal class DeleteService<TEntity, TContext> : BaseEndpointService<TEntity>, IDeleteService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly IGetService<TEntity, TContext> _getService;
        private TContext DbContext { get; }
        
        public DeleteService(
            TContext dbContext,
            IGetService<TEntity, TContext> getService,
            IEntityConfigurationProvider configurationProvider,
            SlothfulOptions options) : base(configurationProvider, options)
        {
            DbContext = dbContext;
            _getService = getService;
        }
        
        public async Task DeleteAsync(object keyProperty)
        {            
            CheckEntityKey(typeof(TEntity), keyProperty);

            var item = await _getService.GetAsync(keyProperty);
            
            DbContext.Set<TEntity>().Remove(item);
            await DbContext.SaveChangesAsync();
        }
    }
}
