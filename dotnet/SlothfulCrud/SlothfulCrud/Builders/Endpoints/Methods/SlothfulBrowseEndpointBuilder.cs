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
        
        public SlothfulBrowseEndpointBuilder Map(Type entityType)
        {
            if (!BuildBrowseQueryType(entityType, out var inputType, out var resultType)) return this;
            
            var mapMethod = GetGenericMapTypedMethod(nameof(MapTypedGet));
            ConventionBuilder = (RouteHandlerBuilder)mapMethod.MakeGenericMethod(inputType).Invoke(this, [
                BuilderParams.WebApplication,
                entityType,
                BuilderParams.DbContextType,
                resultType
            ]);

            return this;
        }

        private bool BuildBrowseQueryType(Type entityType, out Type inputType, out Type resultType)
        {
            PropertyInfo[] parameters = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
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
            
            resultType = typeof(PagedResults<>).MakeGenericType(entityType);
            return true;
        }
        
        private MethodInfo GetGenericMapTypedMethod(string methodName)
        {
            return typeof(SlothfulBrowseEndpointBuilder).GetMethod(methodName);
        }
        
        public void MapTypedGet<T>(
            WebApplication app,
            Type entityType,
            Type dbContextType,
            Type returnType) where T : new()
        {
            app.MapGet(BuilderParams.ApiSegmentProvider.GetApiSegment(entityType.Name) + "/list/{page}", (HttpContext context, [FromRoute] ushort page, [FromQuery] T query) =>
                {
                    query = QueryObjectProvider.PrepareQueryObject<T>(query, context);
                    using var serviceScope = app.Services.CreateScope();
                    var service = SlothfulTypesProvider.GetConcreteOperationService(entityType, dbContextType, serviceScope);
                    return service.Browse(page, query);
                })
                .WithName($"Browse{entityType.Name}s")
                .Produces(200, returnType)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
        }
    }
}