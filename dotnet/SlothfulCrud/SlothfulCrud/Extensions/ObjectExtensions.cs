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
    }
}