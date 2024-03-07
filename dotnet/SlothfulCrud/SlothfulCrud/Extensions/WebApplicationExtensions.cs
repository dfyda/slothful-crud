using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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
                webApplication.MapGet($"/{entityType.Name}s", () =>
                    {
                        var service = webApplication.Services.GetRequiredService(
                            SlothfulTypesProvider.GetConcreteOperationService(executingAssembly, entityType, dbContextType)) as dynamic;
                        return service.Get();
                    })
                    .WithName($"Get{entityType.Name}");
            }
            return webApplication;
        }
    }
}