using System.Reflection;
using SlothfulCrud.Builders.Dynamic;
using SlothfulCrud.Builders.Dynamic.Extensions.Methods;
using SlothfulCrud.Builders.Dynamic.Extensions.Properties;
using SlothfulCrud.Types.Dto;

namespace SlothfulCrud.Types
{
    internal class DynamicType
    {
        public static Type NewDynamicType(
            ParameterInfo[] parameters,
            Type entityType,
            string methodName,
            bool isNullable = false,
            IDictionary<string, Type> additionalProperties = null)
        {
            var typeProperties = parameters.Select(x => new TypeProperty(x.Name, x.ParameterType));
            return NewDynamicType(
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
            return NewDynamicType(
                typeProperties.ToArray(),
                entityType,
                methodName,
                isNullable,
                additionalProperties);
        }
        
        public static Type NewDynamicBrowseQuery(
            PropertyInfo[] parameters,
            Type entityType,
            string methodName,
            IDictionary<string, Type> additionalProperties = null)
        {
            var typeProperties = ReplaceDateToDateRange(parameters);
            typeProperties = RemoveNestedEntities(typeProperties);
            return NewDynamicType(
                typeProperties.ToArray(),
                entityType,
                methodName,
                true,
                additionalProperties);
        }

        private static List<TypeProperty> RemoveNestedEntities(IEnumerable<TypeProperty> typeProperties)
        {
            return typeProperties
                .Where(x => !x.Type.IsClass || x.Type == typeof(string))
                .ToList();
        }

        private static List<TypeProperty> ReplaceDateToDateRange(PropertyInfo[] parameters)
        {
            var typeProperties = parameters
                .Select(x => new TypeProperty(x.Name, x.PropertyType))
                .ToList();
            var dates = typeProperties
                .Where(x => x.Type == typeof(DateTime) || x.Type == typeof(DateTime?))
                .ToList();
            
            if (dates.Count != 0)
            {
                typeProperties = typeProperties
                    .Where(x => x.Type != typeof(DateTime) && x.Type != typeof(DateTime?))
                    .ToList();
                
                foreach (var date in dates)
                {
                    typeProperties.AddRange(new[]
                    {
                        new TypeProperty($"{date.Name}From", typeof(DateTime?)),
                        new TypeProperty($"{date.Name}To", typeof(DateTime?))
                    });
                }
            }

            return typeProperties;
        }

        public static Type NewDynamicType(
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

        public static Type NewDynamicTypeDto(Type entityType, string typeName, bool exposeAll)
        {
            var propertiesToAdd = new List<TypeProperty>();
            var properties = entityType.GetProperties();
            if (exposeAll)
            {
                propertiesToAdd.AddRange(properties.Select(x => new TypeProperty(x.Name, x.PropertyType)));
            }
            else
            {
                var primitiveProperties = properties
                    .Where(x => !x.PropertyType.IsClass || x.PropertyType == typeof(string))
                    .ToList();
                var nestedProperties = properties
                    .Where(x => x.PropertyType.IsClass && x.PropertyType != typeof(string))
                    .ToList();
                
                propertiesToAdd.AddRange(primitiveProperties.Select(x => new TypeProperty(x.Name, x.PropertyType)));
                propertiesToAdd.AddRange(nestedProperties.Select(x => new TypeProperty(x.Name, typeof(BaseEntityDto))));
            }
            
            var builder = new DynamicTypeBuilder();
            return builder
                .DefineType(typeName)
                .AddProperties(propertiesToAdd.ToArray(), false)
                .AddFakeTryParse()
                .Build();
        }

        public static dynamic MapToDto(dynamic item, Type entityType, Type dtoType, bool exposeAll, string keyPropertyType)
        {
            var properties = entityType.GetProperties();
            var nestedProperties = entityType.GetProperties()
                .Where(x => x.PropertyType.IsClass && x.PropertyType != typeof(string))
                .ToList();
            
            var instance = Activator.CreateInstance(dtoType);
            FlatNestedEntities(item, dtoType, properties, nestedProperties, instance, exposeAll, keyPropertyType);

            return instance;
        }

        public static dynamic MapToPagedResultsDto(dynamic item, Type entityType, Type dtoType, bool exposeAll, string keyPropertyType)
        {
            var properties = entityType.GetProperties();
            var nestedProperties = entityType.GetProperties()
                .Where(x => x.PropertyType.IsClass && x.PropertyType != typeof(string))
                .ToList();
            
            var instances = new List<object>();
            var elements = item.Data;
            foreach (var element in elements)
            {
                var instance = Activator.CreateInstance(dtoType);
                FlatNestedEntities(element, dtoType, properties, nestedProperties, instance, exposeAll, keyPropertyType);

                instances.Add(instance);
            }

            return new PagedResults<object>(item.First, item.Total, item.Rows, instances);
        }

        public static PagedResults<BaseEntityDto> MapToPagedBaseEntityDto(dynamic item, string keyPropertyType)
        {
            var instances = new List<BaseEntityDto>();
            var elements = item.Data;
            foreach (var element in elements)
            {
                var propertyKeyValue = element.GetKeyPropertyValue(keyPropertyType);
                var instance = new BaseEntityDto()
                {
                    Id = element.propertyKeyValue,
                    DisplayName = element.DisplayName
                };

                instances.Add(instance);
            }

            return new PagedResults<BaseEntityDto>(item.First, item.Total, item.Rows, instances);
        }

        private static void FlatNestedEntities(
            dynamic item,
            Type dtoType,
            PropertyInfo[] properties,
            List<PropertyInfo> nestedProperties,
            object instance,
            bool exposeAll,
            string keyPropertyType)
        {
            foreach (var property in properties)
            {
                var value = property.GetValue(item);
                if (!exposeAll && nestedProperties.Contains(property) && value != null)
                {
                    var nestedDto = new BaseEntityDto()
                    {
                        Id = property.PropertyType.GetProperty(keyPropertyType).GetValue(value),
                        DisplayName = (string)property.PropertyType.GetProperty("DisplayName").GetValue(value)
                    };
                    dtoType.GetProperty(property.Name).SetValue(instance, nestedDto);
                }
                else
                {
                    dtoType.GetProperty(property.Name).SetValue(instance, value);
                }
            }
        }
    }
}