using System.Reflection;
using SlothfulCrud.Types;

namespace SlothfulCrud.DynamicTypes
{
    public class DynamicTypeBuilder
    {
        public static Type BuildType(ParameterInfo[] parameters, Type entityType, string methodName)
        {
            var typeProperties = parameters.Select(x => new TypeProperty(x.Name, x.ParameterType));
            return BuildType(typeProperties.ToArray(), entityType, methodName);
        }
        
        public static Type BuildType(PropertyInfo[] parameters, Type entityType, string methodName)
        {
            var typeProperties = parameters.Select(x => new TypeProperty(x.Name, x.PropertyType));
            return BuildType(typeProperties.ToArray(), entityType, methodName);
        }
        
        private static Type BuildType(TypeProperty[] parameters, Type entityType, string methodName)
        {
            var typeBuilder = DynamicAssemblyBuilder.DefineType($"{methodName}{entityType.Name}" );

            foreach (var parameter in parameters)
            {
                DynamicAssemblyBuilder.AddAutoProperty(typeBuilder, parameter.Name, parameter.Type);
            }
            
            DynamicAssemblyBuilder.AddFakeTryParse(typeBuilder);

            return typeBuilder.CreateType();
        }
    }
}