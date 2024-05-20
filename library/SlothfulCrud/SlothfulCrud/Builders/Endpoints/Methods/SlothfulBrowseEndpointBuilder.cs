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
    public class SlothfulBrowseEndpointBuilder<TEntity> : SlothfulMethodEndpointRouteBuilder<TEntity> 
        where TEntity : class, ISlothfulEntity
    {
        public SlothfulBrowseEndpointBuilder(
            SlothfulBuilderParams builderParams,
            EndpointsConfiguration endpointsConfiguration,
            IDictionary<string, Type> generatedDynamicTypes,
            SlothEntityBuilder<TEntity> configurationBuilder
        ) : base(builderParams, endpointsConfiguration, generatedDynamicTypes, configurationBuilder)
        {
        }
        
        public SlothfulBrowseEndpointBuilder<TEntity> Map()
        {
            if (!EndpointsConfiguration.Browse.IsEnable) 
                return this;
            
            if (!BuildBrowseQueryType(BuilderParams.EntityType, out var inputType, out var resultType))
                return this;
            
            var mapMethod = GetGenericMapTypedMethod(nameof(MapTypedGet));
            ConventionBuilder = (RouteHandlerBuilder)mapMethod.MakeGenericMethod(inputType).Invoke(this, [BuilderParams.EntityType, resultType]);

            return this;
        }

        private bool BuildBrowseQueryType(Type entityType, out Type inputType, out Type resultType)
        {
            var parameters = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (parameters.Length == 0)
            {
                inputType = null;
                resultType = null;
                return false;
            }
            
            inputType = DynamicType.NewDynamicBrowseQuery(
                parameters,
                entityType,
                "Browse",
                BrowseFields.Fields);
            GeneratedDynamicTypes.Add(inputType.Name, inputType);
            
            var resultDto = BuildGetDtoType();
            resultType = typeof(PagedResults<>).MakeGenericType(resultDto);
            return true;
        }

        private Type GetResultDto(Type entityType)
        {
            var resultDto = GeneratedDynamicTypes[entityType.Name + "Dto"];
            return resultDto;
        }

        private Type BuildGetDtoType()
        {
            var exposeAll = EndpointsConfiguration.Browse.ExposeAllNestedProperties;
            var type = DynamicType.NewDynamicTypeDto(BuilderParams.EntityType, $"{BuilderParams.EntityType}Dto", exposeAll);
            GeneratedDynamicTypes.Add(type.Name, type);
            return type;
        }
        
        private MethodInfo GetGenericMapTypedMethod(string methodName)
        {
            return typeof(SlothfulBrowseEndpointBuilder<TEntity>).GetMethod(methodName);
        }
        
        public void MapTypedGet<T>(Type entityType, Type returnType) where T : new()
        {
            var exposeAll = EndpointsConfiguration.Browse.ExposeAllNestedProperties;
            var endpoint = BuilderParams.WebApplication.MapGet(BuilderParams.ApiSegmentProvider.GetApiSegment(entityType.Name) + "/list/{page}", 
                    (HttpContext context, [FromRoute] ushort page, [FromQuery] T query) =>
                {
                    query = QueryObjectProvider.PrepareQueryObject<T>(query, context);
                    using var serviceScope = BuilderParams.WebApplication.Services.CreateScope();
                    var service = SlothfulTypesProvider.GetConcreteOperationService(entityType, BuilderParams.DbContextType, serviceScope);
                    return DynamicType.MapToPagedResultsDto(service.Browse(page, query), entityType, GetResultDto(entityType), exposeAll, EndpointsConfiguration.Entity.KeyProperty);
                })
                .WithName($"Browse{entityType.Name}s")
                .Produces(200, returnType)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);

            if (EndpointsConfiguration.Browse.IsAuthorizationEnable)
            {
                endpoint.RequireAuthorization(EndpointsConfiguration.Browse.PolicyNames);
            }
        }
    }
}