using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.DynamicTypes;
using SlothfulCrud.Providers;
using SlothfulCrud.Types;

namespace SlothfulCrud.Managers
{
    public class SlothfulCrudManager : ISlothfulCrudManager
    {
        public WebApplication Register(WebApplication webApplication, Type dbContextType, Assembly executingAssembly)
        {
            var entityTypes = SlothfulTypesProvider.GetSlothfulEntityTypes(executingAssembly);
            foreach (var entityType in entityTypes)
            {
                MapGetEndpoint(webApplication, dbContextType, entityType);
                MapBrowseEndpoint(webApplication, dbContextType, entityType);
                MapCreateEndpoint(webApplication, dbContextType, entityType);
                MapUpdateEndpoint(webApplication, dbContextType, entityType);
                MapDeleteEndpoint(webApplication, dbContextType, entityType);
            }

            return webApplication;
        }
        
        private static void MapCreateEndpoint(WebApplication webApplication, Type dbContextType, Type entityType)
        {
            ConstructorInfo constructor = entityType.GetConstructors()
                .FirstOrDefault(x => x.GetParameters().Length > 0);
            if (constructor is null)
            {
                return;
            }

            ParameterInfo[] parameters = constructor.GetParameters();
            Type dynamicType = DynamicTypeBuilder.BuildType(parameters, entityType, "Create");

            var mapMethod = typeof(SlothfulCrudManager).GetMethod(nameof(MapTypedPost));
            mapMethod.MakeGenericMethod(dynamicType).Invoke(null, [
                webApplication,
                entityType,
                dbContextType
            ]);
        }

        private static void MapUpdateEndpoint(WebApplication webApplication, Type dbContextType, Type entityType)
        {
            var updateMethod = entityType.GetMethod("Update");
            if (updateMethod is null)
            {
                return;
            }
            
            ParameterInfo[] parameters = updateMethod.GetParameters();
            Type dynamicType = DynamicTypeBuilder.BuildType(parameters, entityType, "Update");

            var mapMethod = typeof(SlothfulCrudManager).GetMethod(nameof(MapTypedPut));
            mapMethod.MakeGenericMethod(dynamicType).Invoke(null, [
                webApplication,
                entityType,
                dbContextType
            ]);
        }

        private static void MapDeleteEndpoint(WebApplication webApplication, Type dbContextType, Type entityType)
        {
            webApplication.MapDelete(ApiSegmentProvider.GetApiSegment(entityType.Name) + "/{id}", (Guid id) =>
                {
                    var service = GetService(webApplication, dbContextType, entityType);
                    service.Delete(id);
                    return Results.NoContent();
                })
                .WithName($"Delete{entityType.Name}")
                .Produces(204)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
        }

        private static void MapGetEndpoint(WebApplication webApplication, Type dbContextType, Type entityType)
        {
            webApplication.MapGet(ApiSegmentProvider.GetApiSegment(entityType.Name) + "/{id}", (Guid id) =>
                {
                    var service = GetService(webApplication, dbContextType, entityType);
                    return service.Get(id);
                })
                .WithName($"Get{entityType.Name}Details")
                .Produces(200, entityType)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
        }

        private static void MapBrowseEndpoint(WebApplication webApplication, Type dbContextType, Type entityType)
        {
            PropertyInfo[] parameters = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Type dynamicType = DynamicTypeBuilder.BuildType(
                parameters,
                entityType,
                "Browse",
                true,
                BrowseFields.Fields);
            
            var resultType = typeof(PagedResults<>).MakeGenericType(entityType);
            var mapMethod = typeof(SlothfulCrudManager).GetMethod(nameof(MapTypedGet));
            mapMethod.MakeGenericMethod(dynamicType).Invoke(null, [
                webApplication,
                entityType,
                dbContextType,
                resultType
            ]);
        }

        private static dynamic GetService(WebApplication webApplication, Type dbContextType, Type entityType)
        {
            var serviceType = GetScope(webApplication, dbContextType, entityType,
                out var scope);
            var service = scope.ServiceProvider.GetService(serviceType) as dynamic;
            return service;
        }

        private static Type GetScope(WebApplication webApplication, Type dbContextType,
            Type entityType, out IServiceScope scope)
        {
            var serviceType = SlothfulTypesProvider.GetConcreteOperationService(entityType, dbContextType);
            scope = webApplication.Services.CreateScope();
            return serviceType;
        }
        
        public static void MapTypedPost<T>(
            WebApplication app,
            Type entityType,
            Type dbContextType)
        {
            app.MapPost(ApiSegmentProvider.GetApiSegment(entityType.Name), ([FromBody] T command) =>
                {
                    var id = Guid.NewGuid();
                    var service = GetService(app, dbContextType, entityType);
                    service.Create(id, command);
                    return Results.Created($"/{entityType.Name}s/", id);
                })
                .WithName($"Create{entityType.Name}")
                .Produces<Guid>(201)
                .Produces<BadRequestResult>(400);
        }
        
        public static void MapTypedPut<T>(
            WebApplication app,
            Type entityType,
            Type dbContextType)
        {
            app.MapPut(ApiSegmentProvider.GetApiSegment(entityType.Name) + "/{id}", ([FromRoute] Guid id, [FromBody] T command) =>
                {
                    var service = GetService(app, dbContextType, entityType);
                    service.Update(id, command);
                    return Results.NoContent();
                })
                .WithName($"Update{entityType.Name}")
                .Produces(204)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
        }
        
        public static void MapTypedGet<T>(
            WebApplication app,
            Type entityType,
            Type dbContextType,
            Type returnType) where T : new()
        {
            app.MapGet(ApiSegmentProvider.GetApiSegment(entityType.Name) + "/list/{page}", (HttpContext context, [FromRoute] ushort page, [FromQuery] T query) =>
                {
                    query = QueryObjectProvider.PrepareQueryObject<T>(query, context);
                    var service = GetService(app, dbContextType, entityType);
                    return service.Browse(page, query);
                })
                .WithName($"Browse{entityType.Name}s")
                .Produces(200, returnType)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
        }
    }
}