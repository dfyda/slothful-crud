using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Domain;
using SlothfulCrud.Services.Endpoints.Delete;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Services.Endpoints.Post;
using SlothfulCrud.Services.Endpoints.Put;
using SlothfulCrud.Types;

namespace SlothfulCrud.Services
{
    public class EndpointsService<T, TContext> : IEndpointsService<T, TContext> 
        where T : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly IGetService<T, TContext> _getService;
        private readonly IDeleteService<T, TContext> _deleteService;
        private readonly ICreateService<T, TContext> _createService;
        private readonly IUpdateService<T, TContext> _updateService;
        private readonly IBrowseService<T, TContext> _browseService;
        
        public EndpointsService(
            IGetService<T, TContext> getService,
            IDeleteService<T, TContext> deleteService,
            ICreateService<T, TContext> createService,
            IUpdateService<T, TContext> updateService,
            IBrowseService<T, TContext> browseService)
        {
            _getService = getService;
            _deleteService = deleteService;
            _createService = createService;
            _updateService = updateService;
            _browseService = browseService;
        }
        
        public T Get(Guid id)
        {
            return _getService.Get(id);
        }
        
        public void Delete(Guid id)
        {
            _deleteService.Delete(id);
        }

        public Guid Create(Guid id, dynamic command, IServiceScope serviceScope)
        {
            return _createService.Create(id, command, serviceScope);
        }

        public void Update(Guid id, dynamic command, IServiceScope serviceScope)
        {
            _updateService.Update(id, command, serviceScope);
        }

        public PagedResults<T> Browse(ushort page, dynamic query)
        {
            return _browseService.Browse(page, query);
        }
    }
}