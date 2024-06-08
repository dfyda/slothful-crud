using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Builders.Configurations;
using SlothfulCrud.Builders.Endpoints.Parameters;
using SlothfulCrud.Domain;
using SlothfulCrud.Providers;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Endpoints.Methods
{
    internal class SlothfulDeleteEndpointBuilder<TEntity> : SlothfulMethodEndpointRouteBuilder<TEntity>
        where TEntity : class, ISlothfulEntity
    {
        public SlothfulDeleteEndpointBuilder(
            SlothfulBuilderParams builderParams,
            EndpointsConfiguration endpointsConfiguration,
            IDictionary<string, Type> generatedDynamicTypes,
            SlothEntityBuilder<TEntity> configurationBuilder
        ) : base(builderParams, endpointsConfiguration, generatedDynamicTypes, configurationBuilder)
        {
            BuilderParams = builderParams;
        }
        
        public SlothfulDeleteEndpointBuilder<TEntity> Map()
        {
            if (!EndpointsConfiguration.Delete.IsEnable) 
                return this;

            var mapMethod = GetGenericMapTypedMethod(nameof(MapTypedDelete));
            ConventionBuilder = (RouteHandlerBuilder)mapMethod
                .MakeGenericMethod(EndpointsConfiguration.Entity.KeyPropertyType)
                .Invoke(this, [BuilderParams.EntityType]);

            return this;
        }
        
        private MethodInfo GetGenericMapTypedMethod(string methodName)
        {
            return typeof(SlothfulDeleteEndpointBuilder<TEntity>).GetMethod(methodName);
        }
        
        public IEndpointConventionBuilder MapTypedDelete<TKeyType>(Type entityType)
        {
            var endpoint = BuilderParams.WebApplication.MapDelete(BuilderParams.ApiSegmentProvider.GetApiSegment(entityType.Name) + "/{id}", 
                    (TKeyType id) =>
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
            
            if (EndpointsConfiguration.Delete.IsAuthorizationEnable)
            {
                endpoint.RequireAuthorization(EndpointsConfiguration.Delete.PolicyNames);
            }

            return endpoint;
        }
    }
}