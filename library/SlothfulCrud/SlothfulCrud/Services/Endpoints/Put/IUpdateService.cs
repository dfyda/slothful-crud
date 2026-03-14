using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Domain;

namespace SlothfulCrud.Services.Endpoints.Put
{
    internal interface IUpdateService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        Task UpdateAsync(object keyProperty, dynamic command, IServiceScope serviceScope);
    }
}
