using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Builders.Endpoints.Parameters;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Builders.Endpoints.Methods
{
    public class SlothfulDeleteEndpointBuilder : SlothfulEndpointRouteBuilder
    {
        private RouteHandlerBuilder ConventionBuilder { get; set; }

        public SlothfulDeleteEndpointBuilder(SlothfulBuilderParams builderParams) : base(builderParams)
        {
            BuilderParams = builderParams;
        }
        
        public SlothfulDeleteEndpointBuilder Map(Type entityType)
        {
            ConventionBuilder = BuilderParams.WebApplication.MapDelete(BuilderParams.ApiSegmentProvider.GetApiSegment(entityType.Name) + "/{id}", (Guid id) =>
                {
                    using var serviceScope = BuilderParams.WebApplication.Services.CreateScope();
                    var service =
                        SlothfulTypesProvider.GetConcreteOperationService(entityType, BuilderParams.DbContextType, serviceScope);
                    service.Delete(id);
                    return Results.NoContent();
                })
                .WithName($"Delete{entityType.Name}")
                .Produces(204)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);

            return this;
        }
    }
}