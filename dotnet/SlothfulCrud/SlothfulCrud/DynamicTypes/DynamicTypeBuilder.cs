using System.Reflection;

namespace SlothfulCrud.DynamicTypes
{
    public class DynamicTypeBuilder
    {
        public static Type BuildType(ParameterInfo[] parameters, Type entityType)
        {
            var typeBuilder = DynamicAssemblyBuilder.DefineType($"Create{entityType.Name}" );

            foreach (var parameter in parameters)
            {
                DynamicAssemblyBuilder.AddAutoProperty(typeBuilder, parameter.Name, parameter.ParameterType);
            }

            return typeBuilder.CreateType();
        }
    }
}