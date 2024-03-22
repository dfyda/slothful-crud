using System.Reflection;

namespace SlothfulCrud.Builders.Endpoints.Behaviors.ModifyMethod
{
    public class BaseModifyMethodBehavior : IModifyMethodBehavior
    {
        public MethodInfo GetModifyMethod(Type entityType)
        {
            return entityType.GetMethod("Update");
        }
    }
}