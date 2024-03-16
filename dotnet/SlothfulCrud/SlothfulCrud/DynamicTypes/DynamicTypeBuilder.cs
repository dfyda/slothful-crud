using System.Reflection;
using System.Reflection.Emit;
using SlothfulCrud.Types;

namespace SlothfulCrud.DynamicTypes
{
    public class DynamicTypeBuilder
    {
        public static Type BuildType(
            ParameterInfo[] parameters,
            Type entityType,
            string methodName,
            bool isNullable = false,
            IDictionary<string, Type> additionalProperties = null)
        {
            var typeProperties = parameters.Select(x => new TypeProperty(x.Name, x.ParameterType));
            return BuildType(
                typeProperties.ToArray(),
                entityType,
                methodName,
                isNullable,
                additionalProperties);
        }
        
        public static Type BuildType(
            PropertyInfo[] parameters,
            Type entityType,
            string methodName,
            bool isNullable = false,
            IDictionary<string, Type> additionalProperties = null)
        {
            var typeProperties = parameters.Select(x => new TypeProperty(x.Name, x.PropertyType));
            return BuildType(
                typeProperties.ToArray(),
                entityType,
                methodName,
                isNullable,
                additionalProperties);
        }
        
        private static Type BuildType(
            TypeProperty[] parameters,
            Type entityType,
            string methodName,
            bool isNullable = false,
            IDictionary<string, Type> additionalProperties = null)
        {
            var typeBuilder = DynamicAssemblyBuilder.DefineType($"{methodName}{entityType.Name}" );

            foreach (var parameter in parameters)
            {
                AddProperty(
                    typeBuilder,
                    parameter.Name,
                    parameter.Type,
                    isNullable);
            }
            
            AddAdditionalProperties(isNullable, additionalProperties, typeBuilder);
            
            DynamicAssemblyBuilder.AddFakeTryParse(typeBuilder);

            return typeBuilder.CreateType();
        }

        private static void AddAdditionalProperties(bool isNullable, IDictionary<string, Type> additionalProperties, TypeBuilder typeBuilder)
        {
            if (additionalProperties is null)
            {
                return;
            }
            
            foreach (var (propertyName, propertyType) in additionalProperties)
            {
                AddProperty(
                    typeBuilder,
                    propertyName,
                    propertyType,
                    isNullable);
            }
        }

        private static void AddProperty(
            TypeBuilder typeBuilder,
            string propertyName,
            Type propertyType,
            bool isNullable)
        {
            DynamicAssemblyBuilder.AddAutoProperty(
                typeBuilder,
                propertyName,
                propertyType,
                isNullable);
        }
    }
}