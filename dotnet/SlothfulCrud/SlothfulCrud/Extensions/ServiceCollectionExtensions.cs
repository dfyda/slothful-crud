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
        public static IServiceCollection AddSlothfulCrud<T>(this IServiceCollection serviceCollection)
            where T : DbContext
        {
            var entityTypes = SlothfulTypesProvider.GetSlothfulEntityTypes(Assembly.GetEntryAssembly());
            foreach (var entityType in entityTypes)
            {
                var collection = new Dictionary<Type, Type>
                {
                    { typeof(IEndpointsService<,>).MakeGenericType(entityType, typeof(T)), typeof(EndpointsService<,>).MakeGenericType(entityType, typeof(T)) },
                    { typeof(IGetService<,>).MakeGenericType(entityType, typeof(T)), typeof(GetService<,>).MakeGenericType(entityType, typeof(T))},
                    { typeof(IBrowseService<,>).MakeGenericType(entityType, typeof(T)), typeof(BrowseService<,>).MakeGenericType(entityType, typeof(T)) },
                    { typeof(ICreateService<,>).MakeGenericType(entityType, typeof(T)), typeof(CreateService<,>).MakeGenericType(entityType, typeof(T)) },
                    { typeof(IUpdateService<,>).MakeGenericType(entityType, typeof(T)), typeof(UpdateService<,>).MakeGenericType(entityType, typeof(T)) },
                    { typeof(IDeleteService<,>).MakeGenericType(entityType, typeof(T)), typeof(DeleteService<,>).MakeGenericType(entityType, typeof(T)) }
                };
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