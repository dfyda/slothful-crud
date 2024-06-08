using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Providers;
using SlothfulCrud.Services.Endpoints.Get;

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
            IEntityConfigurationProvider configurationProvider) : base(configurationProvider)
        {
            DbContext = dbContext;
            _getService = getService;
        }
        
        public void Delete(object keyProperty)
        {            
            CheckEntityKey(typeof(TEntity), keyProperty);

            var item = _getService.Get(keyProperty);
            
            DbContext.Set<TEntity>().Remove(item);
            DbContext.SaveChanges();
        }
    }
}