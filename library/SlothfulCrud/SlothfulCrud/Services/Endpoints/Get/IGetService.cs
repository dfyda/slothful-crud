using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;

namespace SlothfulCrud.Services.Endpoints.Get
{
    internal interface IGetService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        TEntity Get(object keyProperty);
    }
}