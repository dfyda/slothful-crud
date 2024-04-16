using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Builders.Configurations;
using SlothfulCrud.Builders.Endpoints.Parameters;
using SlothfulCrud.Domain;
using SlothfulCrud.Providers;
using SlothfulCrud.Types;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Endpoints.Methods
{
    public class SlothfulGetEndpointBuilder<TEntity> : SlothfulMethodEndpointRouteBuilder<TEntity>
        where TEntity : class, ISlothfulEntity
    {
        public SlothfulGetEndpointBuilder(
            SlothfulBuilderParams builderParams,
            EndpointsConfiguration endpointsConfiguration,
            IDictionary<string, Type> generatedDynamicTypes,
            SlothEntityBuilder<TEntity> configurationBuilder
            ) : base(builderParams, endpointsConfiguration, generatedDynamicTypes, configurationBuilder)
        {
        }
        
        public SlothfulGetEndpointBuilder<TEntity> Map()
        {
            var mapMethod = GetGenericMapTypedMethod(nameof(MapTypedGet));
            var resultType = BuildGetDtoType();
            ConventionBuilder = (RouteHandlerBuilder)mapMethod.MakeGenericMethod(resultType).Invoke(this, [BuilderParams.EntityType]);

            return this;
        }
        
        private Type BuildGetDtoType()
        {
            var exposeAll = EndpointsConfiguration.Get.ExposeAllNestedProperties;
            var type = DynamicType.NewDynamicTypeDto(BuilderParams.EntityType, $"{BuilderParams.EntityType}DetailsDto", exposeAll);
            GeneratedDynamicTypes.Add(type.Name, type);
            return type;
        }
        
        private MethodInfo GetGenericMapTypedMethod(string methodName)
        {
            return typeof(SlothfulGetEndpointBuilder<TEntity>).GetMethod(methodName);
        }
        
        public IEndpointConventionBuilder MapTypedGet<T>(Type entityType)
        {
            var exposeAll = EndpointsConfiguration.Get.ExposeAllNestedProperties;
            var endpoint = BuilderParams.WebApplication.MapGet(BuilderParams.ApiSegmentProvider.GetApiSegment(entityType.Name) + "/{id}", (Guid id) =>
                {
                    using var serviceScope = BuilderParams.WebApplication.Services.CreateScope();
                    var service =
                        SlothfulTypesProvider.GetConcreteOperationService(entityType, BuilderParams.DbContextType, serviceScope);
                    var item = service.Get(id);
                    var resultDto = DynamicType.MapToDto(item, entityType, typeof(T), exposeAll);
                    return resultDto;
                })
                .WithName($"Get{entityType.Name}Details")
                .RequireAuthorization()
                .Produces(200, typeof(T))
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
            
            if (EndpointsConfiguration.Get.IsAuthorizationEnable)
            {
                endpoint.RequireAuthorization(EndpointsConfiguration.Browse.PolicyNames);
            }

            return endpoint;
        }
    }
}