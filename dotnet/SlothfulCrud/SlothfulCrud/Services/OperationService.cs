using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;

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
        
        public T Get(Guid id)
        {
            CheckEntityKey(typeof(T));
            
            return DbContext.Set<T>()
                .FirstOrDefault(x => EF.Property<Guid>(x, "Id") == id);
        }

        private void CheckEntityKey(Type type)
        {
            if (type.GetProperty("Id") is null)
            {
                throw new ConfigurationException("Entity must have a property named 'Id'");
            };
        }
    }
}