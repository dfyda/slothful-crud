using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;

namespace SlothfulCrud.Services
{
    public interface IOperationService<T, TDbContext> 
        where T : class, ISlothfulEntity, new() 
        where TDbContext : DbContext
    {
        T Get();
    }
}

