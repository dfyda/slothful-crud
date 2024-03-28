using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Extensions;

namespace SlothfulCrud.Services.Endpoints.Get
{
    public class GetService<T, TContext> : IGetService<T, TContext> 
        where T : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private TContext DbContext { get; }
        
        public GetService(TContext dbContext)
        {
            DbContext = dbContext;
        }
        
        // TO DO: Add configuration for entity id parameter type
        public T Get(Guid id)
        {
            CheckEntityKey(typeof(T));
            
            return DbContext.Set<T>()
                .FirstOrDefault(x => EF.Property<Guid>(x, "Id") == id)
                .OrFail($"{typeof(T)}NotFound", $"{typeof(T)} with id '{id}' not found.");  
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