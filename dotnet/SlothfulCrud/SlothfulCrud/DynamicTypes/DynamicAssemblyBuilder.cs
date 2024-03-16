using System.Reflection;
using System.Reflection.Emit;

namespace SlothfulCrud.DynamicTypes;

public class DynamicAssemblyBuilder
{
    private static readonly AssemblyName AssemblyName = new AssemblyName("DynamicAssembly");
    private static readonly AssemblyBuilder AssemblyBuilder =
        AssemblyBuilder.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run);
    private static readonly ModuleBuilder ModuleBuilder = AssemblyBuilder.DefineDynamicModule("DynamicModule");

    public static TypeBuilder DefineType(string typeName)
    {
        return ModuleBuilder.DefineType(typeName, TypeAttributes.Public);
    }

    public static void AddAutoProperty(
        TypeBuilder typeBuilder,
        string propertyName,
        Type propertyType,
        bool isNullable = false)
    {
        var actualPropertyType = propertyType;
        if (isNullable && propertyType.IsValueType)
        {
            actualPropertyType = typeof(Nullable<>).MakeGenericType(propertyType);
        }
        
        var fieldBuilder = typeBuilder.DefineField($"_{propertyName}", actualPropertyType, FieldAttributes.Private);
        var propertyBuilder =
            typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, actualPropertyType, null);

        var getMethodBuilder = typeBuilder.DefineMethod($"get_{propertyName}",
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, actualPropertyType,
            Type.EmptyTypes);
        var getIlGenerator = getMethodBuilder.GetILGenerator();
        getIlGenerator.Emit(OpCodes.Ldarg_0);
        getIlGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
        getIlGenerator.Emit(OpCodes.Ret);

        var setMethodBuilder = typeBuilder.DefineMethod($"set_{propertyName}",
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null,
            new[] { actualPropertyType });
        var setIlGenerator = setMethodBuilder.GetILGenerator();
        setIlGenerator.Emit(OpCodes.Ldarg_0);
        setIlGenerator.Emit(OpCodes.Ldarg_1);
        setIlGenerator.Emit(OpCodes.Stfld, fieldBuilder);
        setIlGenerator.Emit(OpCodes.Ret);

        propertyBuilder.SetGetMethod(getMethodBuilder);
        propertyBuilder.SetSetMethod(setMethodBuilder);
    }

    public static void AddFakeTryParse(TypeBuilder typeBuilder)
    {
        var methodParams = new Type[] { typeof(string), typeof(object).MakeByRefType() };
        MethodBuilder tryParseMethod = typeBuilder.DefineMethod(
            "TryParse",
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(bool),
            methodParams);

        ILGenerator il = tryParseMethod.GetILGenerator();

        var resultVar = il.DeclareLocal(typeof(object));
        var returnValue = il.DeclareLocal(typeof(bool));

        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldloc, resultVar);
        il.Emit(OpCodes.Stind_Ref);

        il.Emit(OpCodes.Ldloc, returnValue);
        il.Emit(OpCodes.Ret);
    }
}