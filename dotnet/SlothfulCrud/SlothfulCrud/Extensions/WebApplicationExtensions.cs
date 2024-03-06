using System.Reflection;
using Microsoft.AspNetCore.Builder;
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
            Assembly executingAssembly)
        {
            var entityTypes = SlothfulTypesProvider.GetSlothfulEntityTypes(executingAssembly);
            foreach (var entityType in entityTypes)
            {
                webApplication.MapGet($"/{entityType.Name}s", () =>
                    {
                        var service = webApplication.Services.GetService(
                            SlothfulTypesProvider.GetConcreteOperationService(executingAssembly, entityType)) as dynamic;
                        return service.Get();
                    })
                    .WithName($"Get{entityType.Name}");
            }
            return webApplication;
        }
    }
}