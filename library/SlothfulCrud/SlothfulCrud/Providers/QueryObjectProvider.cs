using System.Reflection;
using Microsoft.AspNetCore.Http;
using SlothfulCrud.Exceptions;

namespace SlothfulCrud.Providers
{
    internal static class QueryObjectProvider
    {
        public static T PrepareQueryObject<T>(HttpContext context) where T : new()
        {
            var query = new T();
            
            var properties = ReflectionCache.GetProperties(typeof(T));
            foreach (var propertyInfo in properties)
            {
                if (!context.Request.Query.ContainsKey(propertyInfo.Name))
                {
                    continue;
                }

                var parsedValue = ParseQueryParam(context, propertyInfo);
                propertyInfo.SetValue(query, parsedValue);
            }

            return query;
        }

        private static object ParseQueryParam(HttpContext context, PropertyInfo propertyInfo)
        {
            var queryParam = context.Request.Query[propertyInfo.Name].ToString();
            var propertyType = propertyInfo.PropertyType;
            var nullableType = Nullable.GetUnderlyingType(propertyType);
            var targetType = nullableType ?? propertyType;
            var isNullable = nullableType is not null;

            if (targetType == typeof(string))
            {
                return queryParam;
            }

            if (string.IsNullOrWhiteSpace(queryParam) && isNullable)
            {
                return null;
            }

            var parseMethod = ReflectionCache.GetParseMethod(targetType);

            return parseMethod switch
            {
                null => throw new ConfigurationException(
                    $"Parse method not found for type '{propertyInfo.PropertyType.Name}'."),
                _ => parseMethod.Invoke(null, [queryParam])
            };
        }
    }
}
