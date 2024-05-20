using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Domain;
using SlothfulCrud.Types;
using SlothfulCrud.Types.Dto;

namespace SlothfulCrud.Services
{
    public interface IEndpointsService<TEntity, TDbContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TDbContext : DbContext
    {
        TEntity Get(object id);
        void Delete(object id);
        object Create(object id, dynamic command, IServiceScope serviceScope);
        void Update(object id, dynamic command, IServiceScope serviceScope);
        PagedResults<TEntity> Browse(ushort page, dynamic query);
        PagedResults<BaseEntityDto> BrowseSelectable(ushort page, dynamic query);
    }
}

