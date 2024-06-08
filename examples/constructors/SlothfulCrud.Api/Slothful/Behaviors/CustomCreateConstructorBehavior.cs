using System.Reflection;
using SlothfulCrud.Api.Domain;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Exceptions;

namespace SlothfulCrud.Api.Slothful.Behaviors
{
    public class CustomCreateConstructorBehavior : ICreateConstructorBehavior
    {
        public ConstructorInfo GetConstructorInfo(Type entityType)
        {
            if (entityType == typeof(Sloth))
            {
                return typeof(Sloth).GetConstructor(new[] { typeof(Guid), typeof(string) });
            }

            throw new ConfigurationException($"Type: '{entityType.Name}' is not supported.");
        }
    }
}