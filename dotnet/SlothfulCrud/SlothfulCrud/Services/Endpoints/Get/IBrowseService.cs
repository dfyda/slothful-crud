using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Types;

namespace SlothfulCrud.Services.Endpoints.Get
{
    public interface IBrowseService<T, TContext> 
        where T : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        PagedResults<T> Browse(ushort page, dynamic query);
    }
}