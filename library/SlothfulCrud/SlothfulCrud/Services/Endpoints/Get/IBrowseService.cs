using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Types;

namespace SlothfulCrud.Services.Endpoints.Get
{
    internal interface IBrowseService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        PagedResults<TEntity> Browse(ushort page, dynamic query);
    }
}