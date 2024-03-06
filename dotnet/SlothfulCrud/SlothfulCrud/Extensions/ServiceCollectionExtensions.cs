using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Providers;
using SlothfulCrud.Services;

namespace SlothfulCrud.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSlothfulServices(this IServiceCollection serviceCollection, Assembly executingAssembly)
        {
            var entityTypes = SlothfulTypesProvider.GetSlothfulEntityTypes(executingAssembly);
            foreach (var entityType in entityTypes)
            {
                var closedGenericType = typeof(OperationService<>).MakeGenericType(entityType);
                serviceCollection.AddTransient(typeof(IOperationService<>).MakeGenericType(entityType), closedGenericType);
            }
            return serviceCollection;
        }
    }
}