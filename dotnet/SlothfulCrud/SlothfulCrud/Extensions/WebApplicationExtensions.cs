using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.DynamicTypes;
using SlothfulCrud.Providers;
using SlothfulCrud.Types;

namespace SlothfulCrud.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseSlothfulCrud(this WebApplication webApplication, Assembly executingAssembly)
        {
            return webApplication;
        }

        public static WebApplication RegisterSlothfulEndpoints(
            this WebApplication webApplication,
            Type dbContextType,
            Assembly executingAssembly)
        {
            var entityTypes = SlothfulTypesProvider.GetSlothfulEntityTypes(executingAssembly);
            foreach (var entityType in entityTypes)
            {
                MapGetEndpoint(webApplication, dbContextType, executingAssembly, entityType);
                MapBrowseEndpoint(webApplication, dbContextType, executingAssembly, entityType);
                MapCreateEndpoint(webApplication, dbContextType, executingAssembly, entityType);
                MapUpdateEndpoint(webApplication, dbContextType, executingAssembly, entityType);
                MapDeleteEndpoint(webApplication, dbContextType, executingAssembly, entityType);
            }

            return webApplication;
        }

        private static void MapCreateEndpoint(WebApplication webApplication, Type dbContextType, Assembly executingAssembly,
            Type entityType)
        {
            ConstructorInfo constructor = entityType.GetConstructors()
                .FirstOrDefault(x => x.GetParameters().Length > 0);
            if (constructor is null)
            {
                return;
            }

            ParameterInfo[] parameters = constructor.GetParameters();
            Type dynamicType = DynamicTypeBuilder.BuildType(parameters, entityType, "Create");

            var mapMethod = typeof(WebApplicationExtensions).GetMethod(nameof(MapTypedPost));
            mapMethod.MakeGenericMethod(dynamicType).Invoke(null, [
                webApplication,
                entityType,
                dbContextType,
                executingAssembly
            ]);
        }

        private static void MapUpdateEndpoint(WebApplication webApplication, Type dbContextType, Assembly executingAssembly,
            Type entityType)
        {
            var updateMethod = entityType.GetMethod("Update");
            if (updateMethod is null)
            {
                return;
            }
            
            ParameterInfo[] parameters = updateMethod.GetParameters();
            Type dynamicType = DynamicTypeBuilder.BuildType(parameters, entityType, "Update");

            var mapMethod = typeof(WebApplicationExtensions).GetMethod(nameof(MapTypedPut));
            mapMethod.MakeGenericMethod(dynamicType).Invoke(null, [
                webApplication,
                entityType,
                dbContextType,
                executingAssembly
            ]);
        }

        private static void MapDeleteEndpoint(WebApplication webApplication, Type dbContextType, Assembly executingAssembly,
            Type entityType)
        {
            webApplication.MapDelete($"/{entityType.Name}s/" + "{id}", (Guid id) =>
                {
                    var service = GetService(webApplication, dbContextType, executingAssembly, entityType);
                    service.Delete(id);
                    return Results.NoContent();
                })
                .WithName($"Delete{entityType.Name}")
                .Produces(204)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
        }

        private static void MapGetEndpoint(WebApplication webApplication, Type dbContextType, Assembly executingAssembly,
            Type entityType)
        {
            webApplication.MapGet($"/{entityType.Name}s/" + "{id}", (Guid id) =>
                {
                    var service = GetService(webApplication, dbContextType, executingAssembly, entityType);
                    return service.Get(id);
                })
                .WithName($"Get{entityType.Name}Details")
                .Produces(200, entityType)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
        }

        private static void MapBrowseEndpoint(WebApplication webApplication, Type dbContextType, Assembly executingAssembly,
            Type entityType)
        {
            PropertyInfo[] parameters = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var additionalProperties = new Dictionary<string, Type>()
            {
                { "Skip", typeof(ushort) },
                { "Rows", typeof(ushort) },
                { "OrderBy", typeof(string) },
                { "OrderDirection", typeof(string) }
            };
            Type dynamicType = DynamicTypeBuilder.BuildType(
                parameters,
                entityType,
                "Browse",
                true,
                additionalProperties);
            
            var resultType = typeof(PagedResults<>).MakeGenericType(entityType);
            var mapMethod = typeof(WebApplicationExtensions).GetMethod(nameof(MapTypedGet));
            mapMethod.MakeGenericMethod(dynamicType).Invoke(null, [
                webApplication,
                entityType,
                dbContextType,
                resultType,
                executingAssembly
            ]);
        }

        private static dynamic GetService(WebApplication webApplication, Type dbContextType, Assembly executingAssembly,
            Type entityType)
        {
            var serviceType = GetScope(webApplication, dbContextType, executingAssembly, entityType,
                out var scope);
            var service = scope.ServiceProvider.GetService(serviceType) as dynamic;
            return service;
        }

        private static Type GetScope(WebApplication webApplication, Type dbContextType, Assembly executingAssembly,
            Type entityType, out IServiceScope scope)
        {
            var serviceType = SlothfulTypesProvider.GetConcreteOperationService(executingAssembly, entityType,
                dbContextType);
            scope = webApplication.Services.CreateScope();
            return serviceType;
        }
        
        public static void MapTypedPost<T>(
            WebApplication app,
            Type entityType,
            Type dbContextType,
            Assembly executingAssembly)
        {
            app.MapPost($"/{entityType.Name}s/", ([FromBody] T command) =>
                {
                    var id = Guid.NewGuid();
                    var service = GetService(app, dbContextType, executingAssembly, entityType);
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
            Type dbContextType,
            Assembly executingAssembly)
        {
            app.MapPut($"/{entityType.Name}s/" + "{id}", ([FromRoute] Guid id, [FromBody] T command) =>
                {
                    var service = GetService(app, dbContextType, executingAssembly, entityType);
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
            Type returnType,
            Assembly executingAssembly) where T : new()
        {
            app.MapGet($"/{entityType.Name}s/list/" + "{page}", (HttpContext context, [FromRoute] ushort page, [FromQuery] T query) =>
                {
                    query = QueryObjectProvider.PrepareQueryObject<T>(query, context);
                    var service = GetService(app, dbContextType, executingAssembly, entityType);
                    return service.Browse(page, query);
                })
                .WithName($"Browse{entityType.Name}s")
                .Produces(200, returnType)
                .Produces<NotFoundResult>(404)
                .Produces<BadRequestResult>(400);
        }
    }
}