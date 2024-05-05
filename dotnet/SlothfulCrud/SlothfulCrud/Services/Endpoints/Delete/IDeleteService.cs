using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;

namespace SlothfulCrud.Services.Endpoints.Delete
{
    public interface IDeleteService<T, TKeyProperty, TContext> 
        where T : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        void Delete(TKeyProperty id);
    }
}