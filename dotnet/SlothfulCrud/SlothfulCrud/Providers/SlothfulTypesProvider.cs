using System.Reflection;
using SlothfulCrud.Domain;
using SlothfulCrud.Services;

namespace SlothfulCrud.Providers
{
    public static class SlothfulTypesProvider
    {
        public static IEnumerable<Type> GetSlothfulEntityTypes(Assembly executingAssembly)
        {
            var types = executingAssembly.GetTypes();
                
            return types.Where(t => t.GetInterfaces().Contains(typeof(ISlothfulEntity)));
        }
        
        public static Type GetConcreteOperationService(Assembly executingAssembly, Type entityType)
        {
            return typeof(IOperationService<>).MakeGenericType(entityType);
        }
    }
}