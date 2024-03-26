using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Builders.Endpoints.Parameters;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Builders.Endpoints.Methods
{
    public class SlothfulGetEndpointBuilder : SlothfulEndpointRouteBuilder
    {
        private RouteHandlerBuilder ConventionBuilder { get; set; }

        public SlothfulGetEndpointBuilder(SlothfulBuilderParams builderParams) : base(builderParams)
        {
            BuilderParams = builderParams;
        }
        
        public SlothfulGetEndpointBuilder Map(Type entityType)
        {
            ConventionBuilder = BuilderParams.WebApplication.MapGet(BuilderParams.ApiSegmentProvider.GetApiSegment(entityType.Name) + "/{id}", (Guid id) =>
                {
                    using var serviceScope = BuilderParams.WebApplication.Services.CreateScope();
                    var service =
                        SlothfulTypesProvider.GetConcreteOperationService(entityType, BuilderParams.DbContextType, serviceScope);
                    return service.Get(id);
                })
                .WithName($"Get{entityType.Name}Details")
                .Produces(200, entityType)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);

            return this;
        }

        public SlothfulGetEndpointBuilder Produces(int statusCode)
        {
            ConventionBuilder.Produces(statusCode);
            return this;
        }

        public SlothfulGetEndpointBuilder Produces(int statusCode, Type responseType)
        {
            ConventionBuilder.Produces(statusCode, responseType);
            return this;
        }

        public SlothfulGetEndpointBuilder Produces<TResponse>(
            int statusCode = StatusCodes.Status200OK,
            string contentType = null,
            params string[] additionalContentTypes)
        {
            ConventionBuilder.Produces(statusCode, typeof(TResponse), contentType, additionalContentTypes);
            return this;
        }
    }
}