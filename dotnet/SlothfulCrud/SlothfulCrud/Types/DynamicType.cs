using System.Reflection;
using SlothfulCrud.Builders.Dynamic;
using SlothfulCrud.Builders.Dynamic.Extensions.Methods;
using SlothfulCrud.Builders.Dynamic.Extensions.Properties;

namespace SlothfulCrud.Types
{
    public class DynamicType
    {
        public static Type NewDynamicType(
            ParameterInfo[] parameters,
            Type entityType,
            string methodName,
            bool isNullable = false,
            IDictionary<string, Type> additionalProperties = null)
        {
            var typeProperties = parameters.Select(x => new TypeProperty(x.Name, x.ParameterType));
            return BuildType(
                typeProperties.ToArray(),
                entityType,
                methodName,
                isNullable,
                additionalProperties);
        }
        
        public static Type NewDynamicType(
            PropertyInfo[] parameters,
            Type entityType,
            string methodName,
            bool isNullable = false,
            IDictionary<string, Type> additionalProperties = null)
        {
            var typeProperties = parameters.Select(x => new TypeProperty(x.Name, x.PropertyType));
            return BuildType(
                typeProperties.ToArray(),
                entityType,
                methodName,
                isNullable,
                additionalProperties);
        }
        
        private static Type BuildType(
            TypeProperty[] parameters,
            Type entityType,
            string methodName,
            bool isNullable = false,
            IDictionary<string, Type> additionalProperties = null)
        {
            var builder = new DynamicTypeBuilder();
            return builder
                .DefineType($"{methodName}{entityType.Name}")
                .AddProperties(parameters, isNullable)
                .AddAdditionalProperties(additionalProperties, isNullable)
                .AddFakeTryParse()
                .Build();
        }
    }
}