using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;

namespace SlothfulCrud.Services
{
    public class OperationService<T, TDbContext> : IOperationService<T, TDbContext> 
        where T : class, ISlothfulEntity, new() 
        where TDbContext : DbContext
    {
        private TDbContext DbContext { get; }
        
        public OperationService(TDbContext dbContext)
        {
            DbContext = dbContext;
        }
        
        public T Get()
        {
            return DbContext.Set<T>().First();
        }
    }
}