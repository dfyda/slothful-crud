using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Domain;

namespace SlothfulCrud.Services.Endpoints.Put
{
    public interface IUpdateService<T, TKeyProperty, TContext> 
        where T : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        void Update(TKeyProperty id, dynamic command, IServiceScope serviceScope);
    }
}