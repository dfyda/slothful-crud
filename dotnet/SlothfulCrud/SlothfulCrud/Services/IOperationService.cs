using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Types;

namespace SlothfulCrud.Services
{
    public interface IOperationService<T, TDbContext> 
        where T : class, ISlothfulEntity, new() 
        where TDbContext : DbContext
    {
        T Get(Guid id);
        void Delete(Guid id);
        Guid Create(Guid id, dynamic command);
        void Update(Guid id, dynamic command);
        PagedResults<T> Browse(ushort page, dynamic query);
    }
}

