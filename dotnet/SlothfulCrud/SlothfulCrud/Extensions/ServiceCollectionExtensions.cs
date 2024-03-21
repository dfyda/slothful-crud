using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Builders;
using SlothfulCrud.Managers;
using SlothfulCrud.Providers;
using SlothfulCrud.Services;

namespace SlothfulCrud.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSlothfulCrud<T>(this IServiceCollection serviceCollection)
            where T : DbContext
        {
            var entityTypes = SlothfulTypesProvider.GetSlothfulEntityTypes(Assembly.GetEntryAssembly());
            foreach (var entityType in entityTypes)
            {
                var closedGenericType = typeof(OperationService<,>).MakeGenericType(entityType, typeof(T));
                var interfaceType = typeof(IOperationService<,>).MakeGenericType(entityType, typeof(T));
                serviceCollection.AddScoped(interfaceType, closedGenericType);
            }
            return serviceCollection.AddScoped();
        }
        
        private static IServiceCollection AddScoped(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ISlothfulCrudManager, SlothfulCrudManager>();
            serviceCollection.AddScoped<IApiSegmentProvider, ApiSegmentProvider>();
            serviceCollection.AddScoped<ISlothfulEndpointRouteBuilder, SlothfulEndpointRouteBuilder>();

            return serviceCollection;
        }
    }
}