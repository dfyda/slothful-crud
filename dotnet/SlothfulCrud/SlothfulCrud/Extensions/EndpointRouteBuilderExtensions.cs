using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Extensions
{
    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointRouteBuilder RegisterSlothfulEndpoints(this IEndpointRouteBuilder endpointRouteBuilder, Assembly executingAssembly)
        {
            var entityTypes = SlothfulTypesProvider.GetSlothfulEntityTypes(executingAssembly);
            foreach (var entityType in entityTypes)
            {
                endpointRouteBuilder.MapGet($"/{entityType.Name}s", () => entityType.Name)
                    .WithName($"Get{entityType.Name}");
            }
            return endpointRouteBuilder;
        }
    }
}