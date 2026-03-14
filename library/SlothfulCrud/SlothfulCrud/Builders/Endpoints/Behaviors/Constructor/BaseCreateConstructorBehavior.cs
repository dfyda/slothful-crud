using System.Reflection;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Builders.Endpoints.Behaviors.Constructor
{
    internal class BaseCreateConstructorBehavior : ICreateConstructorBehavior
    {
        public ConstructorInfo GetConstructorInfo(Type entityType)
        {
            return ReflectionCache.GetFirstParameterizedConstructor(entityType);
        }
    }
}
