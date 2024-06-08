using System.Reflection;
using System.Reflection.Emit;
using SlothfulCrud.Types;

namespace SlothfulCrud.Builders.Dynamic.Extensions.Properties
{
    internal static class DynamicTypeBuilderExtensions
    {
        public static DynamicTypeBuilder AddProperty(
            this DynamicTypeBuilder dynamicTypeBuilder,
            string propertyName,
            Type propertyType,
            bool isNullable)
        {
            return dynamicTypeBuilder.Do(builder =>
            {
                AddAutoProperty(
                    builder,
                    propertyName,
                    propertyType,
                    isNullable);
            });
        }
        
        public static DynamicTypeBuilder AddProperties(
            this DynamicTypeBuilder dynamicTypeBuilder,
            TypeProperty[] properties,
            bool isNullable)
        {
            return dynamicTypeBuilder.Do(builder =>
            {
                foreach (var property in properties)
                {
                    AddAutoProperty(
                        builder,
                        property.Name,
                        property.Type,
                        isNullable);
                }
            });
        }
        
        public static DynamicTypeBuilder AddAdditionalProperties(
            this DynamicTypeBuilder dynamicTypeBuilder,
            IDictionary<string, Type> additionalProperties,
            bool isNullable)
        {
            return dynamicTypeBuilder.Do(builder =>
            {
                if (additionalProperties is null)
                {
                    return;
                }
            
                foreach (var (propertyName, propertyType) in additionalProperties)
                {
                    AddAutoProperty(
                        builder,
                        propertyName,
                        propertyType,
                        isNullable);
                }
            });
        }
        
        private static void AddAutoProperty(
            TypeBuilder typeBuilder,
            string propertyName,
            Type propertyType,
            bool isNullable = false)
        {
            var actualPropertyType = propertyType;
            if (isNullable && propertyType.IsValueType && !IsAlreadyNullable(propertyType))
            {
                actualPropertyType = typeof(Nullable<>).MakeGenericType(propertyType);
            }

            var fieldBuilder = typeBuilder.DefineField($"_{propertyName}", actualPropertyType, FieldAttributes.Private);
            var propertyBuilder =
                typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, actualPropertyType, null);

            var getMethodBuilder = GetMethodBuilder(typeBuilder, propertyName, actualPropertyType, fieldBuilder);
            var setMethodBuilder = SetMethodBuilder(typeBuilder, propertyName, actualPropertyType, out var setIlGenerator);
            
            setIlGenerator.Emit(OpCodes.Ldarg_0);
            setIlGenerator.Emit(OpCodes.Ldarg_1);
            setIlGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            setIlGenerator.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);
        }
        
        private static bool IsAlreadyNullable(Type propertyType)
        {
            return propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        
        private static MethodBuilder SetMethodBuilder(
            TypeBuilder typeBuilder,
            string propertyName,
            Type actualPropertyType,
            out ILGenerator setIlGenerator)
        {
            var setMethodBuilder = typeBuilder.DefineMethod($"set_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null,
                new[] { actualPropertyType });
            setIlGenerator = setMethodBuilder.GetILGenerator();
            return setMethodBuilder;
        }

        private static MethodBuilder GetMethodBuilder(
            TypeBuilder typeBuilder,
            string propertyName,
            Type actualPropertyType,
            FieldBuilder fieldBuilder)
        {
            var getMethodBuilder = typeBuilder.DefineMethod($"get_{propertyName}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, actualPropertyType,
                Type.EmptyTypes);
            var getIlGenerator = getMethodBuilder.GetILGenerator();
            getIlGenerator.Emit(OpCodes.Ldarg_0);
            getIlGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            getIlGenerator.Emit(OpCodes.Ret);
            return getMethodBuilder;
        }
    }
}
