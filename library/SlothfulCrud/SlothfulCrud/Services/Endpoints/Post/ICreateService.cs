using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Domain;

namespace SlothfulCrud.Services.Endpoints.Post
{
    internal interface ICreateService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        object Create(object keyProperty, dynamic command, IServiceScope serviceScope);
    }
}