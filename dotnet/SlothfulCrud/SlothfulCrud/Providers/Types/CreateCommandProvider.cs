using System.Reflection;
using SlothfulCrud.Extensions;
using SlothfulCrud.Types;

namespace SlothfulCrud.Providers.Types
{
    public static class CreateCommandProvider
    {
        public static Type PrepareCommand(ConstructorInfo constructor, Type entityType)
        {
            var parameters = constructor.GetParameters();
            var nested = entityType.GetProperties()
                .Where(x => x.PropertyType.IsClass && x.PropertyType != typeof(string))
                .ToArray();

            var parametersWithoutNested = ChangeObjectToIdParam(entityType, parameters, nested).ToArray();
            
            return DynamicType.NewDynamicType(parametersWithoutNested, entityType, "Create");
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