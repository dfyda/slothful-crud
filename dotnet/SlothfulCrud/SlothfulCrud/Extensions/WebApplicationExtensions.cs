using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using SlothfulCrud.DynamicTypes;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Providers;

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
                webApplication.MapGet($"/{entityType.Name}s/" + "{id}", (Guid id) =>
                    {
                        var service = GetService(webApplication, dbContextType, executingAssembly, entityType);
                        return service.Get(id);
                    })
                    .WithName($"Get{entityType.Name}Details")
                    .Produces(200, entityType)
                    .Produces<NotFoundResult>(404)
                    .Produces<BadRequestResult>(400);
                
                ConstructorInfo constructor = entityType.GetConstructors()
                    .FirstOrDefault(x => x.GetParameters().Length > 0);
                if (constructor is null)
                {
                    throw new ConfigurationException($"Entity '{entityType.Name}' must have a constructor.");
                }
                
                ParameterInfo[] parameters = constructor.GetParameters();
                Type dynamicType = DynamicTypeBuilder.BuildType(parameters, entityType);
                // var lambda = DynamicLambdaBuilder.CreateLambdaForDynamicType(dynamicType, entityType);
                webApplication.MapPost($"/{entityType.Name}s", async (HttpContext context) =>
                    {
                        var data = await context.Request.ReadFromJsonAsync(dynamicType);
                        var service = GetService(webApplication, dbContextType, executingAssembly, entityType);
                        var id = Guid.NewGuid();
                        service.Create(id);
                        return Results.Created($"/{entityType.Name}s/{id}", id);
                    })
                    .WithName($"Create{entityType.Name}")
                    .Produces<Guid>(201)
                    .Produces<BadRequestResult>(400);
            }
            return webApplication;
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
    }
}