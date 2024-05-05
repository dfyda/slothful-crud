using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Services.Endpoints.Get;

namespace SlothfulCrud.Services.Endpoints.Delete
{
    public class DeleteService<T, TKeyProperty, TContext> : IDeleteService<T, TKeyProperty, TContext> 
        where T : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly IGetService<T, TKeyProperty, TContext> _getService;
        private TContext DbContext { get; }
        
        public DeleteService(TContext dbContext, IGetService<T, TKeyProperty, TContext> getService)
        {
            DbContext = dbContext;
            _getService = getService;
        }
        
        public void Delete(TKeyProperty id)
        {
            var item = _getService.Get(id);
            
            DbContext.Set<T>().Remove(item);
            DbContext.SaveChanges();
        }
    }
}