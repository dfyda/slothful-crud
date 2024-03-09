using System.Reflection;
using System.Reflection.Emit;

namespace SlothfulCrud.DynamicTypes;

public class DynamicAssemblyBuilder
{
    private static readonly AssemblyName AssemblyName = new AssemblyName("DynamicAssembly");
    private static readonly AssemblyBuilder AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run);
    private static readonly ModuleBuilder ModuleBuilder = AssemblyBuilder.DefineDynamicModule("DynamicModule");

    public static TypeBuilder DefineType(string typeName)
    {
        return ModuleBuilder.DefineType(typeName, TypeAttributes.Public);
    }

    public static void AddAutoProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
    {
        var fieldBuilder = typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);
        var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
        
        var getMethodBuilder = typeBuilder.DefineMethod($"get_{propertyName}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
        var getIlGenerator = getMethodBuilder.GetILGenerator();
        getIlGenerator.Emit(OpCodes.Ldarg_0);
        getIlGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
        getIlGenerator.Emit(OpCodes.Ret);

        var setMethodBuilder = typeBuilder.DefineMethod($"set_{propertyName}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new[] { propertyType });
        var setIlGenerator = setMethodBuilder.GetILGenerator();
        setIlGenerator.Emit(OpCodes.Ldarg_0);
        setIlGenerator.Emit(OpCodes.Ldarg_1);
        setIlGenerator.Emit(OpCodes.Stfld, fieldBuilder);
        setIlGenerator.Emit(OpCodes.Ret);

        propertyBuilder.SetGetMethod(getMethodBuilder);
        propertyBuilder.SetSetMethod(setMethodBuilder);
    }
}