using System.Dynamic;
using System.Reflection;

namespace SlothfulCrud.Extensions
{
    public static class ObjectExtensions
    {
        public static IDictionary<string, object> GetProperties(this object obj)
        {
            if (obj is ExpandoObject expandoObject)
            {
                return expandoObject.ToDictionary(kv => kv.Key, kv => kv.Value);
            }

            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var propertyValues = new Dictionary<string, object>();

            foreach (var property in properties)
            {
                propertyValues[property.Name] = property.GetValue(obj);
            }

            return propertyValues;
        }
        
        public static object GetKeyPropertyValue(this object obj, string keyProperty)
        {
            var properties = obj.GetProperties();
            var keyPropertyValue = properties.FirstOrDefault(x => x.Key == keyProperty)
                .OrFail($"KeyPropertyNotFound", $"Key property '{keyProperty}' not found on type '{obj.GetType().Name}'.");
            
            return keyPropertyValue.Value;
        }
    }
}