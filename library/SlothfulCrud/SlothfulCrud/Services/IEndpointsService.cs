using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Domain;
using SlothfulCrud.Types;
using SlothfulCrud.Types.Dto;

namespace SlothfulCrud.Services
{
    internal interface IEndpointsService<TEntity, TDbContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TDbContext : DbContext
    {
        Task<TEntity> GetAsync(object id);
        Task DeleteAsync(object id);
        Task<object> CreateAsync(object id, dynamic command, IServiceScope serviceScope);
        Task UpdateAsync(object id, dynamic command, IServiceScope serviceScope);
        Task<PagedResults<TEntity>> BrowseAsync(ushort page, dynamic query);
        Task<PagedResults<BaseEntityDto>> BrowseSelectableAsync(ushort page, dynamic query);
    }
}
