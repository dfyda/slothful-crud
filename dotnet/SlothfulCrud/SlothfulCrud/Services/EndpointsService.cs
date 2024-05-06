using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Domain;
using SlothfulCrud.Services.Endpoints.Delete;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Services.Endpoints.Post;
using SlothfulCrud.Services.Endpoints.Put;
using SlothfulCrud.Types;
using SlothfulCrud.Types.Dto;

namespace SlothfulCrud.Services
{
    public class EndpointsService<TEntity, TContext> : IEndpointsService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly IGetService<TEntity, TContext> _getService;
        private readonly IDeleteService<TEntity, TContext> _deleteService;
        private readonly ICreateService<TEntity, TContext> _createService;
        private readonly IUpdateService<TEntity, TContext> _updateService;
        private readonly IBrowseService<TEntity, TContext> _browseService;
        private readonly IBrowseSelectableService<TEntity, TContext> _browseSelectableService;
        
        public EndpointsService(
            IGetService<TEntity, TContext> getService,
            IDeleteService<TEntity, TContext> deleteService,
            ICreateService<TEntity,TContext> createService,
            IUpdateService<TEntity, TContext> updateService,
            IBrowseService<TEntity, TContext> browseService,
            IBrowseSelectableService<TEntity, TContext> browseSelectableService)
        {
            _getService = getService;
            _deleteService = deleteService;
            _createService = createService;
            _updateService = updateService;
            _browseService = browseService;
            _browseSelectableService = browseSelectableService;
        }
        
        public TEntity Get(object id)
        {
            return _getService.Get(id);
        }
        
        public void Delete(object id)
        {
            _deleteService.Delete(id);
        }

        public object Create(object id, dynamic command, IServiceScope serviceScope)
        {
            return _createService.Create(id, command, serviceScope);
        }

        public void Update(object id, dynamic command, IServiceScope serviceScope)
        {
            _updateService.Update(id, command, serviceScope);
        }

        public PagedResults<TEntity> Browse(ushort page, dynamic query)
        {
            return _browseService.Browse(page, query);
        }

        public PagedResults<BaseEntityDto> BrowseSelectable(ushort page, dynamic query)
        {
            return _browseSelectableService.Browse(page, query);
        }
    }
}