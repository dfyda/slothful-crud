using System.Collections.Concurrent;
using System.Reflection;

namespace SlothfulCrud.Providers
{
    internal static class ReflectionCache
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Properties = new();
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> NavigationProperties = new();
        private static readonly ConcurrentDictionary<(Type, string), MethodInfo> Methods = new();
        private static readonly ConcurrentDictionary<Type, MethodInfo> ParseMethods = new();
        private static readonly ConcurrentDictionary<Type, ConstructorInfo> Constructors = new();
        private static readonly ConcurrentDictionary<(Type, string), PropertyInfo> SingleProperties = new();

        private static readonly MethodInfo[] QueryableMethods = typeof(Queryable).GetMethods();

        public static PropertyInfo[] GetProperties(Type type)
        {
            return Properties.GetOrAdd(type, static t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
        }

        public static PropertyInfo[] GetNavigationProperties(Type type)
        {
            return NavigationProperties.GetOrAdd(type, static t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.PropertyType.IsClass && p.PropertyType != typeof(string))
                    .ToArray());
        }

        public static MethodInfo GetMethod(Type type, string methodName)
        {
            return Methods.GetOrAdd((type, methodName), static key =>
                key.Item1.GetMethod(key.Item2));
        }

        public static MethodInfo GetParseMethod(Type type)
        {
            return ParseMethods.GetOrAdd(type, static t =>
                t.GetMethods()
                    .FirstOrDefault(x => x.Name == "Parse"
                                         && x.GetParameters().Length == 1
                                         && x.GetParameters()[0].ParameterType == typeof(string)));
        }

        public static ConstructorInfo GetFirstParameterizedConstructor(Type type)
        {
            return Constructors.GetOrAdd(type, static t =>
                t.GetConstructors()
                    .FirstOrDefault(x => x.GetParameters().Length > 0));
        }

        public static PropertyInfo GetProperty(Type type, string propertyName)
        {
            return SingleProperties.GetOrAdd((type, propertyName), static key =>
                key.Item1.GetProperty(key.Item2));
        }

        public static MethodInfo GetQueryableMethod(string methodName, int parameterCount)
        {
            return QueryableMethods
                .First(m => m.Name == methodName && m.GetParameters().Length == parameterCount);
        }
    }
}
