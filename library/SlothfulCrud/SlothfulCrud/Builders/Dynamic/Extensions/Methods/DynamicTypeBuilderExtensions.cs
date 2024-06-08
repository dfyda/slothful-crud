using System.Reflection;
using System.Reflection.Emit;

namespace SlothfulCrud.Builders.Dynamic.Extensions.Methods
{
    internal static class DynamicTypeBuilderExtensions
    {
        public static DynamicTypeBuilder AddFakeTryParse(this DynamicTypeBuilder dynamicTypeBuilder)
        {
            return dynamicTypeBuilder.Do(builder =>
            {
                AddFakeTryParse(builder);
            });
        }
        
        private static void AddFakeTryParse(TypeBuilder typeBuilder)
        {
            var methodParams = new Type[] { typeof(string), typeof(object).MakeByRefType() };
            var tryParseMethod = typeBuilder.DefineMethod(
                "TryParse",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof(bool),
                methodParams);

            var il = tryParseMethod.GetILGenerator();

            var resultVar = il.DeclareLocal(typeof(object));
            var returnValue = il.DeclareLocal(typeof(bool));

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldloc, resultVar);
            il.Emit(OpCodes.Stind_Ref);

            il.Emit(OpCodes.Ldloc, returnValue);
            il.Emit(OpCodes.Ret);
        }
    }
}
