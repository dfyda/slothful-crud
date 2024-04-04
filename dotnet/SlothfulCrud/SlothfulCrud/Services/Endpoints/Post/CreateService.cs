using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Extensions;

namespace SlothfulCrud.Services.Endpoints.Post
{
    public class CreateService<T, TContext> : ICreateService<T, TContext> 
        where T : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly ICreateConstructorBehavior _createConstructorBehavior;
        private TContext DbContext { get; }
        
        public CreateService(TContext dbContext, ICreateConstructorBehavior createConstructorBehavior)
        {
            DbContext = dbContext;
            _createConstructorBehavior = createConstructorBehavior;
        }
        
        public Guid Create(Guid id, dynamic command)
        {
            var constructor = _createConstructorBehavior.GetConstructorInfo(typeof(T));
            if (constructor is null)
            {
                throw new ConfigurationException($"Entity '{typeof(T).Name}' must have a constructor.");
            }
            
            var constructorArgs = constructor.GetParameters().ToArray()
                .Select(param => param.GetDomainMethodParam((object)command, DbContext))
                .ToArray();

            constructorArgs[0] = id;
            
            var instanceOfT = (T)constructor.Invoke(constructorArgs);
            
            DbContext.Set<T>().Add(instanceOfT);
            DbContext.SaveChanges();
            
            return id;
        }
    }
}