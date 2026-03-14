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
    internal class EndpointsService<TEntity, TContext> : IEndpointsService<TEntity, TContext> 
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
        
        public Task<TEntity> GetAsync(object id)
        {
            return _getService.GetAsync(id);
        }
        
        public Task DeleteAsync(object id)
        {
            return _deleteService.DeleteAsync(id);
        }

        public Task<object> CreateAsync(object id, dynamic command, IServiceScope serviceScope)
        {
            return _createService.CreateAsync(id, command, serviceScope);
        }

        public Task UpdateAsync(object id, dynamic command, IServiceScope serviceScope)
        {
            return _updateService.UpdateAsync(id, command, serviceScope);
        }

        public Task<PagedResults<TEntity>> BrowseAsync(ushort page, dynamic query)
        {
            return _browseService.BrowseAsync(page, query);
        }

        public Task<PagedResults<BaseEntityDto>> BrowseSelectableAsync(ushort page, dynamic query)
        {
            return _browseSelectableService.BrowseAsync(page, query);
        }
    }
}
