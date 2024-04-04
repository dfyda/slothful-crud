using System.Reflection;
using SlothfulCrud.Extensions;
using SlothfulCrud.Types;

namespace SlothfulCrud.Providers.Types
{
    public static class CommandProvider
    {
        public static Type PrepareCreateCommand(ConstructorInfo constructor, Type entityType)
        {
            var parameters = constructor.GetParameters();
            return NewCommandType(entityType, parameters, "Create");
        }
        
        public static Type PrepareUpdateCommand(MethodInfo methodInfo, Type entityType)
        {
            var parameters = methodInfo.GetParameters();
            return NewCommandType(entityType, parameters, "Update");
        }

        private static Type NewCommandType(Type entityType, ParameterInfo[] parameters, string methodName)
        {
            var nested = entityType.GetProperties()
                .Where(x => x.PropertyType.IsClass && x.PropertyType != typeof(string))
                .ToArray();

            var parametersWithoutNested = ChangeObjectToIdParam(entityType, parameters, nested).ToArray();
            
            return DynamicType.NewDynamicType(parametersWithoutNested, entityType, methodName);
        }

        private static IEnumerable<TypeProperty> ChangeObjectToIdParam(Type entityType, ParameterInfo[] parameters, PropertyInfo[] nested)
        {
            if (nested.Length == 0)
            {
                return parameters.Select(x => new TypeProperty(x.Name, x.ParameterType));
            }
            
            var idFromNestedUsedInConstructor = parameters
                .Where(x => nested.Any(y => y.PropertyType == x.ParameterType))
                .Select(x => $"{x.Name.FirstCharToUpper()}Id");
            var parametersWithoutNested = parameters
                .Where(x => nested.Any(y => y.PropertyType != x.ParameterType));
            var nestedIdProperties = entityType.GetProperties()
                .Where(x => idFromNestedUsedInConstructor.Contains(x.Name));
            
            return parametersWithoutNested.Select(x => new TypeProperty(x.Name, x.ParameterType))
                .Concat(nestedIdProperties.Select(x => new TypeProperty(x.Name.FirstCharToLower(), x.PropertyType)));
        }
    }
}