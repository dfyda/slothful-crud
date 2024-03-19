using System.Reflection;
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
        
        public static Type GetConcreteOperationService(Type entityType, Type dbContextType)
        {
            return typeof(IOperationService<,>).MakeGenericType(entityType, dbContextType);
        }
    }
}