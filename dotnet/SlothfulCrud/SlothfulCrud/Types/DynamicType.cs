using System.Reflection;
using SlothfulCrud.Builders.Dynamic;
using SlothfulCrud.Builders.Dynamic.Extensions.Methods;
using SlothfulCrud.Builders.Dynamic.Extensions.Properties;
using SlothfulCrud.Types.Dto;

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

        public static dynamic NewDynamicDto(dynamic item, Type entityType, string typeName)
        {
            // TODO: Add configuration for exposing all nested properties
            var properties = entityType.GetProperties();
            var primitiveProperties = properties
                .Where(x => !x.PropertyType.IsClass || x.PropertyType == typeof(string))
                .ToList();
            var nestedProperties = properties
                .Where(x => x.PropertyType.IsClass && x.PropertyType != typeof(string))
                .ToList();
            
            var builder = new DynamicTypeBuilder();
            var type = builder
                .DefineType(typeName)
                .AddProperties(primitiveProperties.Select(x => new TypeProperty(x.Name, x.PropertyType)).ToArray(), false)
                .AddProperties(nestedProperties.Select(x => new TypeProperty(x.Name, typeof(BaseEntityDto))).ToArray(), false)
                .AddFakeTryParse()
                .Build();
            
            var instance = Activator.CreateInstance(type);
            foreach (var property in properties)
            {
                var value = property.GetValue(item);
                if (nestedProperties.Contains(property))
                {
                    var nestedDto = new BaseEntityDto()
                    {
                        Id = (Guid)property.PropertyType.GetProperty("Id").GetValue(value),
                        DisplayName = (string)property.PropertyType.GetProperty("DisplayName").GetValue(value)
                    };
                    type.GetProperty(property.Name).SetValue(instance, nestedDto);
                }
                else
                {
                    type.GetProperty(property.Name).SetValue(instance, value);
                }
            }

            return instance;
        }
    }
}