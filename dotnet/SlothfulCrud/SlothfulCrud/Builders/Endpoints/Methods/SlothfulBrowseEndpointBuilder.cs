using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Providers;
using SlothfulCrud.Types;

namespace SlothfulCrud.Builders.Endpoints.Methods
{
    public class SlothfulBrowseEndpointBuilder
    {
        private readonly WebApplication _webApplication;
        private readonly Type _dbContextType;
        private readonly IApiSegmentProvider _apiSegmentProvider;

        public SlothfulBrowseEndpointBuilder(
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
            if (!BuildBrowseQueryType(entityType, out var inputType, out var resultType)) return;
            
            var mapMethod = GetGenericMapTypedMethod(nameof(MapTypedGet));
            return (IEndpointConventionBuilder)mapMethod.MakeGenericMethod(inputType).Invoke(this, [
                _webApplication,
                entityType,
                _dbContextType,
                resultType
            ]);
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
            return typeof(SlothfulEndpointRouteBuilder).GetMethod(methodName);
        }
        
        public void MapTypedGet<T>(
            WebApplication app,
            Type entityType,
            Type dbContextType,
            Type returnType) where T : new()
        {
            app.MapGet(_apiSegmentProvider.GetApiSegment(entityType.Name) + "/list/{page}", (HttpContext context, [FromRoute] ushort page, [FromQuery] T query) =>
                {
                    query = QueryObjectProvider.PrepareQueryObject<T>(query, context);
                    using var serviceScope = app.Services.CreateScope();
                    var service = SlothfulTypesProvider.GetConcreteOperationService(dbContextType, entityType, serviceScope);
                    return service.Browse(page, query);
                })
                .WithName($"Browse{entityType.Name}s")
                .Produces(200, returnType)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
        }
    }
}