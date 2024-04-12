using System.Reflection;
using SlothfulCrud.Domain;

namespace SlothfulCrud.Builders.Configurations
{
    public class SlothConfigurationBuilder
    {
        private readonly ICollection<object> _builders = new List<object>();

        public SlothConfigurationBuilder ApplyConfigurationsFromAssembly(Assembly assembly)
        {
            var applyEntityConfigurationMethod = typeof(SlothConfigurationBuilder)
                .GetMethods()
                .Single(
                    methodInfo => methodInfo is { Name: nameof(ApplyConfiguration), ContainsGenericParameters: true }
                                  && methodInfo.GetParameters().SingleOrDefault()?.ParameterType
                                      .GetGenericTypeDefinition()
                                  == typeof(ISlothEntityConfiguration<>));

            var configurationInterfaceType = typeof(ISlothEntityConfiguration<>);
            var typesToConfigure = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == configurationInterfaceType))
                .ToList();

            foreach (var type in typesToConfigure)
            {
                var configurationInterfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == configurationInterfaceType)
                    .ToList();

                foreach (var configurationInterface in configurationInterfaces)
                {
                    var genericApplyConfigurationMethod = applyEntityConfigurationMethod
                        .MakeGenericMethod(configurationInterface.GetGenericArguments()[0]);

                    genericApplyConfigurationMethod.Invoke(this, new[] { Activator.CreateInstance(type) });
                }
            }

            return this;
        }

        public SlothConfigurationBuilder ApplyConfiguration<TEntity>(ISlothEntityConfiguration<TEntity> configuration)
            where TEntity : class, ISlothfulEntity
        {
            var builder = new SlothEntityBuilder<TEntity>();
            configuration.Configure(builder);
            _builders.Add(builder);

            return this;
        }

        public SlothEntityBuilder<TEntity> GetBuilder<TEntity>()
            where TEntity : class, ISlothfulEntity
        {
            var builder =
                _builders.SingleOrDefault(builder => builder is SlothEntityBuilder<TEntity>);
            if (builder is null)
            {
                return new SlothEntityBuilder<TEntity>();
            }

            return builder as SlothEntityBuilder<TEntity>;
        }
    }
}