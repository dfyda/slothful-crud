using System.Reflection;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Extensions;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Services.Endpoints.Post
{
    internal class CreateService<TEntity, TContext> : BaseEndpointService<TEntity>, ICreateService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly ICreateConstructorBehavior _createConstructorBehavior;
        private TContext DbContext { get; }
        
        public CreateService(
            TContext dbContext,
            ICreateConstructorBehavior createConstructorBehavior,
            IEntityConfigurationProvider configurationProvider) : base(configurationProvider)
        {
            DbContext = dbContext;
            _createConstructorBehavior = createConstructorBehavior;
        }
        
        public object Create(object keyProperty, dynamic command, IServiceScope serviceScope)
        {
            CheckEntityKey(typeof(TEntity), keyProperty);

            var constructor = GetEntityConstructor();
            var constructorArgs = PrepareConstructorArguments(keyProperty, command, constructor);

            var item = (TEntity)constructor.Invoke(constructorArgs);
            
            if (EntityConfiguration.HasValidation)
            {
                SlothfulTypesProvider.GetConcreteValidator<TEntity>(serviceScope).ValidateAndThrow(item);
            }
            
            DbContext.Set<TEntity>().Add(item);
            DbContext.SaveChanges();
            
            return keyProperty;
        }

        private object[] PrepareConstructorArguments(object keyProperty, dynamic command, ConstructorInfo constructor)
        {
            var constructorArgs = constructor.GetParameters().ToArray()
                .Select(param => param.GetDomainMethodParam((object)command, DbContext))
                .ToArray()
                .OrFail($"{typeof(TEntity).Name}ConstructorArgs",
                    $"Constructor arguments for '{typeof(TEntity).Name}' not found.");

            // Key property must be the first argument in the constructor
            constructorArgs[0] = keyProperty;
            return constructorArgs;
        }

        private ConstructorInfo GetEntityConstructor()
        {
            var constructor = _createConstructorBehavior.GetConstructorInfo(typeof(TEntity));
            if (constructor is null)
            {
                throw new ConfigurationException($"Entity '{typeof(TEntity).Name}' must have a constructor.");
            }

            return constructor;
        }
    }
}