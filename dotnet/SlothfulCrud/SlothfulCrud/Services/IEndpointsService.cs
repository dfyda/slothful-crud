using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Domain;
using SlothfulCrud.Types;

namespace SlothfulCrud.Services
{
    public interface IEndpointsService<T, TDbContext> 
        where T : class, ISlothfulEntity, new() 
        where TDbContext : DbContext
    {
        T Get(Guid id);
        void Delete(Guid id);
        Guid Create(Guid id, dynamic command, IServiceScope serviceScope);
        void Update(Guid id, dynamic command, IServiceScope serviceScope);
        PagedResults<T> Browse(ushort page, dynamic query);
    }
}

