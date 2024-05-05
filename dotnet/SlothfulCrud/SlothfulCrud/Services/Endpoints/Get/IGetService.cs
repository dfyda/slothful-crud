using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;

namespace SlothfulCrud.Services.Endpoints.Get
{
    public interface IGetService<T, TKeyProperty, TContext> 
        where T : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        T Get(TKeyProperty id);
    }
}