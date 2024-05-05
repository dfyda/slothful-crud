using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Domain;
using SlothfulCrud.Types;
using SlothfulCrud.Types.Dto;

namespace SlothfulCrud.Services
{
    public interface IEndpointsService<TEntity, TKeyProperty, TDbContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TDbContext : DbContext
    {
        TEntity Get(TKeyProperty id);
        void Delete(TKeyProperty id);
        TKeyProperty Create(TKeyProperty id, dynamic command, IServiceScope serviceScope);
        void Update(TKeyProperty id, dynamic command, IServiceScope serviceScope);
        PagedResults<TEntity> Browse(ushort page, dynamic query);
        PagedResults<BaseEntityDto> BrowseSelectable(ushort page, dynamic query);
    }
}

