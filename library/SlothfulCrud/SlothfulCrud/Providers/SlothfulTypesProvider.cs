using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Domain;
using SlothfulCrud.Services;

namespace SlothfulCrud.Providers
{
    public static class SlothfulTypesProvider
    {
        public static IEnumerable<Type> GetSlothfulEntityTypes(Assembly assembly)
        {
            var types = assembly.GetTypes();
                
            return types.Where(t => t.GetInterfaces().Contains(typeof(ISlothfulEntity)));
        }
        
        public static dynamic GetConcreteOperationService(
            Type entityType,
            Type dbContextType,
            IServiceScope serviceScope)
        {
            var serviceType = GetConcreteOperationServiceType(entityType, dbContextType);
            return serviceScope.ServiceProvider.GetService(serviceType);
        }
        
        public static IValidator<TEntityType> GetConcreteValidator<TEntityType>(IServiceScope serviceScope)
        {
            return serviceScope.ServiceProvider.GetService<IValidator<TEntityType>>();
        }
        
        public static dynamic GetEntityPropertyKeyValueProvider(Type entityType, IServiceScope serviceScope)
        {
            var serviceType = typeof(IEntityPropertyKeyValueProvider<>).MakeGenericType(entityType);
            return serviceScope.ServiceProvider.GetService(serviceType);
        }

        private static Type GetConcreteOperationServiceType(Type entityType, Type dbContextType)
        {
            return typeof(IEndpointsService<,>).MakeGenericType(entityType, dbContextType);
        }
    }
}