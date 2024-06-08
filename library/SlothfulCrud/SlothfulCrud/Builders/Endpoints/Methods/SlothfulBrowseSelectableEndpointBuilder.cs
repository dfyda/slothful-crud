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
using SlothfulCrud.Types.Dto;

namespace SlothfulCrud.Builders.Endpoints.Methods
{
    internal class SlothfulBrowseSelectableEndpointBuilder<TEntity> : SlothfulMethodEndpointRouteBuilder<TEntity> 
        where TEntity : class, ISlothfulEntity
    {
        public SlothfulBrowseSelectableEndpointBuilder(
            SlothfulBuilderParams builderParams,
            EndpointsConfiguration endpointsConfiguration,
            IDictionary<string, Type> generatedDynamicTypes,
            SlothEntityBuilder<TEntity> configurationBuilder
        ) : base(builderParams, endpointsConfiguration, generatedDynamicTypes, configurationBuilder)
        {
        }
        
        public SlothfulBrowseSelectableEndpointBuilder<TEntity> Map()
        {
            if (!EndpointsConfiguration.BrowseSelectable.IsEnable) 
                return this;
            
            if (!BuildBrowseQueryType(BuilderParams.EntityType, out var inputType, out var resultType))
                return this;
            
            var mapMethod = GetGenericMapTypedMethod(nameof(MapTypedGet));
            ConventionBuilder = (RouteHandlerBuilder)mapMethod.MakeGenericMethod(inputType).Invoke(this, [BuilderParams.EntityType, resultType]);

            return this;
        }
        
        private bool BuildBrowseQueryType(Type entityType, out Type inputType, out Type resultType)
        {
            var parameters = new List<TypeProperty>()
            {
                new TypeProperty("Search", typeof(string))
            };
            
            inputType = DynamicType.NewDynamicType(
                parameters.ToArray(),
                entityType,
                "BrowseSelectable",
                true,
                BrowseFields.Fields);
            GeneratedDynamicTypes.Add(inputType.Name, inputType);
            
            var resultDto = typeof(BaseEntityDto);
            resultType = typeof(PagedResults<>).MakeGenericType(resultDto);
            return true;
        }
        
        private MethodInfo GetGenericMapTypedMethod(string methodName)
        {
            return typeof(SlothfulBrowseSelectableEndpointBuilder<TEntity>).GetMethod(methodName);
        }
        
        public void MapTypedGet<T>(Type entityType, Type returnType) where T : new()
        {
            var endpoint = BuilderParams.WebApplication.MapGet(BuilderParams.ApiSegmentProvider.GetApiSegment(entityType.Name) + "/selectable-list/{page}", 
                    (HttpContext context, [FromRoute] ushort page, [FromQuery] T query) =>
                    {
                        query = QueryObjectProvider.PrepareQueryObject<T>(context);
                        using var serviceScope = BuilderParams.WebApplication.Services.CreateScope();
                        var service = SlothfulTypesProvider.GetConcreteOperationService(entityType, BuilderParams.DbContextType, serviceScope);
                        return service.BrowseSelectable(page, query);
                    })
                .WithName($"BrowseSelectable{entityType.Name}s")
                .Produces(200, returnType)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);

            if (EndpointsConfiguration.BrowseSelectable.IsAuthorizationEnable)
            {
                endpoint.RequireAuthorization(EndpointsConfiguration.BrowseSelectable.PolicyNames);
            }
        }
    }
}