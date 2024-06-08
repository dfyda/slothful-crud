using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Types;
using SlothfulCrud.Types.Dto;

namespace SlothfulCrud.Services.Endpoints.Get
{
    internal interface IBrowseSelectableService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        PagedResults<BaseEntityDto> Browse(ushort page, dynamic query);
    }
}