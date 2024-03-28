using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Services.Endpoints.Get;

namespace SlothfulCrud.Services.Endpoints.Delete
{
    public class DeleteService<T, TContext> : IDeleteService<T, TContext> 
        where T : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly IGetService<T, TContext> _getService;
        private TContext DbContext { get; }
        
        public DeleteService(TContext dbContext, IGetService<T, TContext> getService)
        {
            DbContext = dbContext;
            _getService = getService;
        }
        
        public void Delete(Guid id)
        {
            var item = _getService.Get(id);
            
            DbContext.Set<T>().Remove(item);
            DbContext.SaveChanges();
        }
    }
}