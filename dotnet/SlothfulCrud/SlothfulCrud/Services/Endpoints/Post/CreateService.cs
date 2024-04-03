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
            
            var constructorArgs = constructor.GetParameters()
                .Select(param => GetConstructorParam(command, param))
                .ToArray();

            constructorArgs[0] = id;
            
            var instanceOfT = (T)constructor.Invoke(constructorArgs);
            
            DbContext.Set<T>().Add(instanceOfT);
            DbContext.SaveChanges();
            
            return id;
        }

        private object GetConstructorParam(dynamic command, ParameterInfo param)
        {
            if (param.ParameterType.IsAssignableTo(typeof(ISlothfulEntity)))
            {
                var entityIdProperty = ((object)command).GetProperties()[$"{param.Name.FirstCharToLower()}Id"];
                return GetEntityItem(param.ParameterType,
                    (Guid)(entityIdProperty ?? Guid.Empty), entityIdProperty is null);
            }
            return ((object)command).GetProperties()[param.Name];
        }

        private object GetEntityItem(Type entityType, Guid id, bool isNullable = false)
        {
            if (id == Guid.Empty && isNullable)
            {
                return null;
            }
            
            var item = DbContext.Find(entityType, id);
            if (item is null && !isNullable)
            {
                throw new NotFoundException($"{entityType.Name.ToLowerInvariant()}_not_found", 
                    $"Entity '{entityType.Name}' with id '{id}' not found.");
            }

            return item;
        }
    }
}