using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Builders.Endpoints.Parameters;
using SlothfulCrud.Providers;
using SlothfulCrud.Types;

namespace SlothfulCrud.Builders.Endpoints.Methods
{
    public class SlothfulBrowseEndpointBuilder : SlothfulMethodEndpointRouteBuilder
    {
        public SlothfulBrowseEndpointBuilder(SlothfulBuilderParams builderParams) : base(builderParams)
        {
            BuilderParams = builderParams;
        }
        
        public SlothfulBrowseEndpointBuilder Map()
        {
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
            
            inputType = DynamicType.NewDynamicType(
                parameters,
                entityType,
                "Browse",
                true,
                BrowseFields.Fields);
            GeneratedDynamicTypes.Add(inputType.Name, inputType);
            
            var resultDto = GetResultDto(entityType);
            resultType = typeof(PagedResults<>).MakeGenericType(resultDto);
            return true;
        }

        private Type GetResultDto(Type entityType)
        {
            var resultDto = GeneratedDynamicTypes[entityType.Name + "DetailsDto"];
            return resultDto;
        }

        private MethodInfo GetGenericMapTypedMethod(string methodName)
        {
            return typeof(SlothfulBrowseEndpointBuilder).GetMethod(methodName);
        }
        
        public void MapTypedGet<T>(Type entityType, Type returnType) where T : new()
        {
            BuilderParams.WebApplication.MapGet(BuilderParams.ApiSegmentProvider.GetApiSegment(entityType.Name) + "/list/{page}", 
                    (HttpContext context, [FromRoute] ushort page, [FromQuery] T query) =>
                {
                    query = QueryObjectProvider.PrepareQueryObject<T>(query, context);
                    using var serviceScope = BuilderParams.WebApplication.Services.CreateScope();
                    var service = SlothfulTypesProvider.GetConcreteOperationService(entityType, BuilderParams.DbContextType, serviceScope);
                    return DynamicType.MapToPagedResultsDto(service.Browse(page, query), entityType, GetResultDto(entityType));
                })
                .WithName($"Browse{entityType.Name}s")
                .Produces(200, returnType)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
        }
    }
}