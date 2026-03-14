using System.Dynamic;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Extensions
{
    internal static class ObjectExtensions
    {
        public static IDictionary<string, object> GetProperties(this object obj)
        {
            if (obj is ExpandoObject expandoObject)
            {
                return expandoObject.ToDictionary(kv => kv.Key, kv => kv.Value);
            }

            var type = obj.GetType();
            var properties = ReflectionCache.GetProperties(type);
            var propertyValues = new Dictionary<string, object>(properties.Length);

            foreach (var property in properties)
            {
                propertyValues[property.Name] = property.GetValue(obj);
            }

            return propertyValues;
        }
        
        public static object GetKeyPropertyValue(this object obj, string keyProperty)
        {
            var property = ReflectionCache.GetProperty(obj.GetType(), keyProperty);
            if (property is null)
            {
                throw new KeyNotFoundException($"Key property '{keyProperty}' not found on type '{obj.GetType().Name}'.");
            }
            return property.GetValue(obj);
        }
    }
}
