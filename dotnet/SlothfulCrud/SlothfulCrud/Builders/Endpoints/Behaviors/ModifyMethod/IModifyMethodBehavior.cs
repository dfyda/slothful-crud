using System.Reflection;

namespace SlothfulCrud.Builders.Endpoints.Behaviors.ModifyMethod
{
    public interface IModifyMethodBehavior
    {
        MethodInfo GetModifyMethod(Type entityType);
    }
}