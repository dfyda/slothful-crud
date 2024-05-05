using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Domain;

namespace SlothfulCrud.Services.Endpoints.Post
{
    public interface ICreateService<T, TKeyProperty, TContext> 
        where T : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        TKeyProperty Create(TKeyProperty id, dynamic command, IServiceScope serviceScope);
    }
}