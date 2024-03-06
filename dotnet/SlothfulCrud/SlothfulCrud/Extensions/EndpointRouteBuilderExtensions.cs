using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Extensions
{
    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointRouteBuilder RegisterSlothfulEndpointsOld(
            this IEndpointRouteBuilder endpointRouteBuilder,
            WebApplication app,
            Assembly executingAssembly)
        {
            var entityTypes = SlothfulTypesProvider.GetSlothfulEntityTypes(executingAssembly);
            foreach (var entityType in entityTypes)
            {
                endpointRouteBuilder.MapGet($"/{entityType.Name}s", () =>
                    {
                        var service = app.Services.GetService(
                            SlothfulTypesProvider.GetConcreteOperationService(executingAssembly, entityType)) as dynamic;
                        return service.Get();
                    })
                    .WithName($"Get{entityType.Name}");
            }
            return endpointRouteBuilder;
        }
    }
}