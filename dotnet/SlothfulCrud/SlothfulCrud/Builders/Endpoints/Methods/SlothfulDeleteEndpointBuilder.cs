using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Builders.Configurations;
using SlothfulCrud.Builders.Endpoints.Parameters;
using SlothfulCrud.Domain;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Builders.Endpoints.Methods
{
    public class SlothfulDeleteEndpointBuilder<TEntity> : SlothfulMethodEndpointRouteBuilder<TEntity>
        where TEntity : class, ISlothfulEntity
    {
        public SlothfulDeleteEndpointBuilder(
            SlothfulBuilderParams builderParams,
            SlothfulEndpointConfigurationBuilder<TEntity> configurationBuilder) : base(builderParams, configurationBuilder)
        {
            BuilderParams = builderParams;
        }
        
        public SlothfulDeleteEndpointBuilder<TEntity> Map()
        {
            var entityType = BuilderParams.EntityType;
            ConventionBuilder = BuilderParams.WebApplication.MapDelete(BuilderParams.ApiSegmentProvider.GetApiSegment(entityType.Name) + "/{id}", 
                    (Guid id) =>
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