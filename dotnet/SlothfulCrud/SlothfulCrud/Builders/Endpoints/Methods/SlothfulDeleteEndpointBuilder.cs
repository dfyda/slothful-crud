using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Builders.Endpoints.Methods
{
    public class SlothfulDeleteEndpointBuilder
    {
        private readonly WebApplication _webApplication;
        private readonly Type _dbContextType;
        private readonly IApiSegmentProvider _apiSegmentProvider;

        public SlothfulDeleteEndpointBuilder(
            WebApplication webApplication,
            Type dbContextType,
            IApiSegmentProvider apiSegmentProvider)
        {
            _webApplication = webApplication;
            _dbContextType = dbContextType;
            _apiSegmentProvider = apiSegmentProvider;
        }
        
        public IEndpointConventionBuilder Map(Type entityType)
        {
            return _webApplication.MapDelete(_apiSegmentProvider.GetApiSegment(entityType.Name) + "/{id}", (Guid id) =>
                {
                    using var serviceScope = _webApplication.Services.CreateScope();
                    var service =
                        SlothfulTypesProvider.GetConcreteOperationService(entityType, _dbContextType, serviceScope);
                    service.Delete(id);
                    return Results.NoContent();
                })
                .WithName($"Delete{entityType.Name}")
                .Produces(204)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
        }
    }
}