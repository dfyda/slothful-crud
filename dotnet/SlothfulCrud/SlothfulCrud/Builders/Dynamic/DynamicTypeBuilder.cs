using System.Reflection;
using System.Reflection.Emit;

namespace SlothfulCrud.Builders.Dynamic
{
    public class DynamicTypeBuilder
    {
        private static readonly AssemblyName AssemblyName = new AssemblyName("DynamicAssembly");
        private static readonly AssemblyBuilder AssemblyBuilder =
            AssemblyBuilder.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run);
        private static readonly ModuleBuilder ModuleBuilder = AssemblyBuilder.DefineDynamicModule("DynamicModule");
        
        private readonly ICollection<Func<TypeBuilder, TypeBuilder>> _actions = new List<Func<TypeBuilder, TypeBuilder>>();
        private string TypeName { get; set;}
        
        public DynamicTypeBuilder Do(Action<TypeBuilder> action)
        {
            return AddAction(action);
        }
        
        public DynamicTypeBuilder DefineType(string typeName) {
            TypeName = typeName;
            return this;
        }

        public Type Build()
        {
            return _actions
                .Aggregate(ModuleBuilder.DefineType(TypeName, TypeAttributes.Public), (builder, action) => action(builder))
                .CreateType();
        }

        private DynamicTypeBuilder AddAction(Action<TypeBuilder> action)
        {
            _actions.Add(builder =>
            {
                action(builder);
                return builder;
            });
            return this;
        }
    }
}