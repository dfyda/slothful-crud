using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;

namespace SlothfulCrud.Services.Endpoints.Delete
{
    internal interface IDeleteService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        void Delete(object keyProperty);
    }
}