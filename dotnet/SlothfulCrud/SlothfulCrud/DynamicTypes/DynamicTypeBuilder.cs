using System.Reflection;

namespace SlothfulCrud.DynamicTypes
{
    public class DynamicTypeBuilder
    {
        public static Type BuildType(ParameterInfo[] parameters, Type entityType, string methodName)
        {
            var typeBuilder = DynamicAssemblyBuilder.DefineType($"{methodName}{entityType.Name}" );

            foreach (var parameter in parameters)
            {
                DynamicAssemblyBuilder.AddAutoProperty(typeBuilder, parameter.Name, parameter.ParameterType);
            }

            return typeBuilder.CreateType();
        }
    }
}