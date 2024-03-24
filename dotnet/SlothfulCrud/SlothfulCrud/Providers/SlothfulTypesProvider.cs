using System.Reflection;
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
        
        public static dynamic GetConcreteOperationService(Type dbContextType, Type entityType, IServiceScope serviceScope)
        {
            var serviceType = GetConcreteOperationServiceType(entityType, dbContextType);
            return serviceScope.ServiceProvider.GetService(serviceType);
        }

        private static Type GetConcreteOperationServiceType(Type entityType, Type dbContextType)
        {
            return typeof(IOperationService<,>).MakeGenericType(entityType, dbContextType);
        }
    }
}