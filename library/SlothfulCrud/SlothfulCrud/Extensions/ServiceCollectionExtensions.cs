using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Builders.Endpoints.Behaviors.ModifyMethod;
using SlothfulCrud.Exceptions.Handlers;
using SlothfulCrud.Managers;
using SlothfulCrud.Providers;
using SlothfulCrud.Services;
using SlothfulCrud.Services.Endpoints.Delete;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Services.Endpoints.Post;
using SlothfulCrud.Services.Endpoints.Put;

namespace SlothfulCrud.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSlothfulCrud<TDbContext>(this IServiceCollection serviceCollection)
            where TDbContext : DbContext
        {
            var entityTypes = SlothfulTypesProvider.GetSlothfulEntityTypes(Assembly.GetEntryAssembly());
            foreach (var entityType in entityTypes)
            {
                var services = GetDynamicServicesCollection<TDbContext>(entityType);
                var providers = GetDynamicProvidersCollection(entityType);
                var collection = services.Union(providers);
                
                foreach (var pair in collection)
                {
                    serviceCollection.AddScoped(pair.Key, pair.Value);
                }
            }
            return serviceCollection
                .AddScoped()
                .AddSingleton()
                .AddTransient()
                .AddBehaviors();
        }

        private static Dictionary<Type, Type> GetDynamicServicesCollection<TDbContext>(Type entityType) where TDbContext : DbContext
        {
            var collection = new Dictionary<Type, Type>
            {
                { typeof(IEndpointsService<,>).MakeGenericType(entityType, typeof(TDbContext)), typeof(EndpointsService<,>).MakeGenericType(entityType, typeof(TDbContext)) },
                { typeof(IGetService<,>).MakeGenericType(entityType, typeof(TDbContext)), typeof(GetService<,>).MakeGenericType(entityType, typeof(TDbContext))},
                { typeof(IBrowseService<,>).MakeGenericType(entityType, typeof(TDbContext)), typeof(BrowseService<,>).MakeGenericType(entityType, typeof(TDbContext)) },
                { typeof(IBrowseSelectableService<,>).MakeGenericType(entityType, typeof(TDbContext)), typeof(BrowseSelectableService<,>).MakeGenericType(entityType, typeof(TDbContext)) },
                { typeof(ICreateService<,>).MakeGenericType(entityType, typeof(TDbContext)), typeof(CreateService<,>).MakeGenericType(entityType, typeof(TDbContext)) },
                { typeof(IUpdateService<,>).MakeGenericType(entityType, typeof(TDbContext)), typeof(UpdateService<,>).MakeGenericType(entityType, typeof(TDbContext)) },
                { typeof(IDeleteService<,>).MakeGenericType(entityType, typeof(TDbContext)), typeof(DeleteService<,>).MakeGenericType(entityType, typeof(TDbContext)) }
                
            };
            return collection;
        }

        private static Dictionary<Type, Type> GetDynamicProvidersCollection(Type entityType)
        {
            var collection = new Dictionary<Type, Type>
            {
                { typeof(IEntityPropertyKeyValueProvider<>).MakeGenericType(entityType), typeof(EntityPropertyKeyValueProvider<>).MakeGenericType(entityType) }
                
            };
            return collection;
        }

        private static IServiceCollection AddScoped(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ISlothfulCrudManager, SlothfulCrudManager>();
            serviceCollection.AddScoped<IApiSegmentProvider, ApiSegmentProvider>();

            return serviceCollection;
        }
        
        private static IServiceCollection AddSingleton(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IEntityConfigurationProvider, EntityConfigurationProvider>();

            return serviceCollection;
        }
        
        private static IServiceCollection AddTransient(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IExceptionHandler, ExceptionHandler>();

            return serviceCollection;
        }
        
        private static IServiceCollection AddBehaviors(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ICreateConstructorBehavior, BaseCreateConstructorBehavior>();
            serviceCollection.AddScoped<IModifyMethodBehavior, BaseModifyMethodBehavior>();

            return serviceCollection;
        }
    }
}