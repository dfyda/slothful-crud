using System.Reflection;
using System.Reflection.Emit;
using SlothfulCrud.Builders.Abstract;

namespace SlothfulCrud.Builders.Dynamic
{
    internal class DynamicTypeBuilder : AbstractFunctionalBuilder<TypeBuilder, DynamicTypeBuilder>
    {
        private static readonly AssemblyName AssemblyName = new AssemblyName("DynamicAssembly");
        private static readonly AssemblyBuilder AssemblyBuilder =
            AssemblyBuilder.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run);
        private static readonly ModuleBuilder ModuleBuilder = AssemblyBuilder.DefineDynamicModule("DynamicModule");
        private string TypeName { get; set; }
        
        public DynamicTypeBuilder DefineType(string typeName) {
            TypeName = typeName;
            return this;
        }

        public Type Build()
        {
            return Actions
                .Aggregate(ModuleBuilder.DefineType(TypeName, TypeAttributes.Public), (builder, action) => action(builder))
                .CreateType();
        }
    }
}