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
            var validatorType = typeof(IValidator<TEntityType>);
            return serviceScope.ServiceProvider.GetService<IValidator<TEntityType>>();
        }

        private static Type GetConcreteOperationServiceType(Type entityType, Type dbContextType)
        {
            return typeof(IEndpointsService<,>).MakeGenericType(entityType, dbContextType);
        }
    }
}