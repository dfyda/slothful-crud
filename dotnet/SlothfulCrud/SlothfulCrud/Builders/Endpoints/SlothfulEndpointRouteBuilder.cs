using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Builders.Endpoints.Behaviors.ModifyMethod;
using SlothfulCrud.DynamicTypes;
using SlothfulCrud.Providers;
using SlothfulCrud.Types;

namespace SlothfulCrud.Builders.Endpoints
{
    public class SlothfulEndpointRouteBuilder : ISlothfulEndpointRouteBuilder
    {
        private readonly IApiSegmentProvider _apiSegmentProvider;
        private readonly ICreateConstructorBehavior _createConstructorBehavior;
        private readonly IModifyMethodBehavior _modifyMethodBehavior;

        public SlothfulEndpointRouteBuilder(
            IApiSegmentProvider apiSegmentProvider,
            ICreateConstructorBehavior createConstructorBehavior,
            IModifyMethodBehavior modifyMethodBehavior)
        {
            _apiSegmentProvider = apiSegmentProvider;
            _createConstructorBehavior = createConstructorBehavior;
            _modifyMethodBehavior = modifyMethodBehavior;
        }
        
        public void MapEndpoints(WebApplication webApplication, Type dbContextType, Type entityType)
        {
            MapGetEndpoint(webApplication, dbContextType, entityType);
            MapBrowseEndpoint(webApplication, dbContextType, entityType);
            MapCreateEndpoint(webApplication, dbContextType, entityType);
            MapUpdateEndpoint(webApplication, dbContextType, entityType);
            MapDeleteEndpoint(webApplication, dbContextType, entityType);
        }
        
        public void MapCreateEndpoint(WebApplication webApplication, Type dbContextType, Type entityType)
        {
            if (!BuildCreateCommandType(entityType, out var inputType)) return;

            var mapMethod = GetGenericMapTypedMethod(nameof(MapTypedPost));
            mapMethod.MakeGenericMethod(inputType).Invoke(this, [
                webApplication,
                entityType,
                dbContextType
            ]);
        }

        private bool BuildCreateCommandType(Type entityType, out Type inputType)
        {
            var constructor = _createConstructorBehavior.GetConstructorInfo(entityType);
            if (constructor is null)
            {
                inputType = null;
                return false;
            }

            var parameters = constructor.GetParameters();
            inputType = DynamicTypeBuilder.BuildType(parameters, entityType, "Create");
            return true;
        }

        public void MapUpdateEndpoint(WebApplication webApplication, Type dbContextType, Type entityType)
        {
            if (!BuildModifyMethodType(entityType, out var inputType)) return;

            var mapMethod = GetGenericMapTypedMethod(nameof(MapTypedPut));
            mapMethod.MakeGenericMethod(inputType).Invoke(this, [
                webApplication,
                entityType,
                dbContextType
            ]);
        }

        private bool BuildModifyMethodType(Type entityType, out Type inputType)
        {
            var modifyMethod = _modifyMethodBehavior.GetModifyMethod(entityType);
            if (modifyMethod is null)
            {
                inputType = null;
                return false;
            }
            
            ParameterInfo[] parameters = modifyMethod.GetParameters();
            inputType = DynamicTypeBuilder.BuildType(parameters, entityType, "Update");
            return true;
        }

        public void MapDeleteEndpoint(WebApplication app, Type dbContextType, Type entityType)
        {
            // TO DO: Add configuration for entity id parameter type
            app.MapDelete(_apiSegmentProvider.GetApiSegment(entityType.Name) + "/{id}", (Guid id) =>
                {
                    using var serviceScope = app.Services.CreateScope();
                    var service = GetService(dbContextType, entityType, serviceScope);
                    service.Delete(id);
                    return Results.NoContent();
                })
                .WithName($"Delete{entityType.Name}")
                // TO DO: Add configuration for custom produces for all endpoints
                .Produces(204)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
        }

        public void MapGetEndpoint(WebApplication webApplication, Type dbContextType, Type entityType)
        {
            webApplication.MapGet(_apiSegmentProvider.GetApiSegment(entityType.Name) + "/{id}", (Guid id) =>
                {
                    using var serviceScope = webApplication.Services.CreateScope();
                    var service = GetService(dbContextType, entityType, serviceScope);
                    return service.Get(id);
                })
                .WithName($"Get{entityType.Name}Details")
                .Produces(200, entityType)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
        }

        public void MapBrowseEndpoint(WebApplication app, Type dbContextType, Type entityType)
        {
            if (!BuildBrowseQueryType(entityType, out var inputType, out var resultType)) return;
            
            var mapMethod = GetGenericMapTypedMethod(nameof(MapTypedGet));
            mapMethod.MakeGenericMethod(inputType).Invoke(this, [
                app,
                entityType,
                dbContextType,
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
            
            inputType = DynamicTypeBuilder.BuildType(
                parameters,
                entityType,
                "Browse",
                true,
                BrowseFields.Fields);
            
            resultType = typeof(PagedResults<>).MakeGenericType(entityType);
            return true;
        }

        private dynamic GetService(Type dbContextType, Type entityType, IServiceScope serviceScope)
        {
            var serviceType = SlothfulTypesProvider.GetConcreteOperationService(entityType, dbContextType);
            return serviceScope.ServiceProvider.GetService(serviceType);
        }
        
        public void MapTypedPost<T>(
            WebApplication app,
            Type entityType,
            Type dbContextType)
        {
            app.MapPost(_apiSegmentProvider.GetApiSegment(entityType.Name), ([FromBody] T command) =>
                {
                    var id = Guid.NewGuid();
                    using var serviceScope = app.Services.CreateScope();
                    var service = GetService(dbContextType, entityType, serviceScope);
                    service.Create(id, command);
                    return Results.Created($"/{entityType.Name}s/", id);
                })
                .WithName($"Create{entityType.Name}")
                .Produces<Guid>(201)
                .Produces<BadRequestResult>(400);
        }
        
        public void MapTypedPut<T>(
            WebApplication app,
            Type entityType,
            Type dbContextType)
        {
            app.MapPut(_apiSegmentProvider.GetApiSegment(entityType.Name) + "/{id}", ([FromRoute] Guid id, [FromBody] T command) =>
                {
                    using var serviceScope = app.Services.CreateScope();
                    var service = GetService(dbContextType, entityType, serviceScope);
                    service.Update(id, command);
                    return Results.NoContent();
                })
                .WithName($"Update{entityType.Name}")
                .Produces(204)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
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
                    var service = GetService(dbContextType, entityType, serviceScope);
                    return service.Browse(page, query);
                })
                .WithName($"Browse{entityType.Name}s")
                .Produces(200, returnType)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
        }
    }
}