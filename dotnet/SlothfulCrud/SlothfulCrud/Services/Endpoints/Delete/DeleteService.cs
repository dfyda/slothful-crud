using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Services.Endpoints.Get;

namespace SlothfulCrud.Services.Endpoints.Delete
{
    public class DeleteService<TEntity, TContext> : IDeleteService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly IGetService<TEntity, TContext> _getService;
        private TContext DbContext { get; }
        
        public DeleteService(TContext dbContext, IGetService<TEntity, TContext> getService)
        {
            DbContext = dbContext;
            _getService = getService;
        }
        
        public void Delete(object id)
        {
            var item = _getService.Get(id);
            
            DbContext.Set<TEntity>().Remove(item);
            DbContext.SaveChanges();
        }
    }
}