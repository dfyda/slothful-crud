using System.Reflection;
using Microsoft.AspNetCore.Http;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Extensions;

namespace SlothfulCrud.Providers
{
    public static class QueryObjectProvider
    {
        public static T PrepareQueryObject<T>(T query, HttpContext context) where T : new()
        {
            query = new T();
            
            var properties = typeof(T).GetProperties();
            foreach (var propertyInfo in properties)
            {
                if (!context.Request.Query.ContainsKey(propertyInfo.Name))
                {
                    continue;
                }

                var parsedValue = ParseQueryParam<T>(context, propertyInfo);
                propertyInfo.SetValue(query, parsedValue);
            }

            return query;
        }

        private static dynamic ParseQueryParam<T>(HttpContext context, PropertyInfo propertyInfo) where T : new()
        {
            var baseType = GetBaseType<T>(propertyInfo);
            
            var parseMethod = baseType
                .PropertyType
                .GetMethods()
                .FirstOrDefault(x => x.Name == "Parse");
                
            var queryParam = context.Request.Query[propertyInfo.Name].ToString();

            return parseMethod switch
            {
                null when propertyInfo.PropertyType == typeof(string) => queryParam,
                null => throw new ConfigurationException(
                    $"Parse method not found for type '{propertyInfo.PropertyType.Name}'."),
                _ => parseMethod.Invoke(null, [queryParam])
            };
        }

        private static PropertyInfo GetBaseType<T>(PropertyInfo propertyInfo) where T : new()
        {
            if (propertyInfo.PropertyType == typeof(string))
            {
                return propertyInfo;
            }
            
            return propertyInfo.PropertyType
                .GetProperties()
                .FirstOrDefault(x => x.Name == "Value")
                .OrFail("PropertyValueNotFound",
                    $"Property 'Value' not found on type '{propertyInfo.PropertyType.Name}'.");
        }
    }
}
