using System.Reflection;

namespace SlothfulCrud.Builders.Endpoints.Behaviors.Constructor
{
    public interface ICreateConstructorBehavior
    {
        ConstructorInfo GetConstructorInfo(Type entityType);
    }
}