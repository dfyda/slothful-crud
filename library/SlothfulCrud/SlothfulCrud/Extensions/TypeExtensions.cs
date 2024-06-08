namespace SlothfulCrud.Extensions
{
    internal static class TypeExtensions
    {
        public static bool IsNullable(this Type type)
        {
            if (Nullable.GetUnderlyingType(type) != null)
                return true;
    
            return !type.IsValueType;
        }
    }
}