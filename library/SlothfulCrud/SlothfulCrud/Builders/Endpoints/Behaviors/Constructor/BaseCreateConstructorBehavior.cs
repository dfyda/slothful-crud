using System.Reflection;

namespace SlothfulCrud.Builders.Endpoints.Behaviors.Constructor
{
    internal class BaseCreateConstructorBehavior : ICreateConstructorBehavior
    {
        public ConstructorInfo GetConstructorInfo(Type entityType)
        {
            return entityType.GetConstructors()
                .FirstOrDefault(x => x.GetParameters().Length > 0);
        }
    }
}