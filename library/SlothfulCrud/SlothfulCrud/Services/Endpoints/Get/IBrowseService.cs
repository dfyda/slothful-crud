using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Types;

namespace SlothfulCrud.Services.Endpoints.Get
{
    internal interface IBrowseService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        Task<PagedResults<TEntity>> BrowseAsync(ushort page, dynamic query);
    }
}
