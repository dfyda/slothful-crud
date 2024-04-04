using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;

namespace SlothfulCrud.Extensions
{
    public static class ParameterInfoExtensions
    {
        public static object GetDomainMethodParam<TContext>(this ParameterInfo param, object command, TContext dbContext)
            where TContext : DbContext
        {
            if (param.ParameterType.IsAssignableTo(typeof(ISlothfulEntity)))
            {
                var entityIdProperty = command.GetProperties()[$"{param.Name.FirstCharToLower()}Id"];
                return GetEntityItem(dbContext, param.ParameterType,
                    (Guid)(entityIdProperty ?? Guid.Empty), entityIdProperty is null);
            }
            return command.GetProperties()[param.Name];
        }

        private static object GetEntityItem<TContext>(TContext dbContext, Type entityType, Guid id, bool isNullable = false)
             where TContext : DbContext
        {
            if (id == Guid.Empty && isNullable)
            {
                return null;
            }
            
            var item = dbContext.Find(entityType, id);
            if (item is null && !isNullable)
            {
                throw new NotFoundException($"{entityType.Name.ToLowerInvariant()}_not_found", 
                    $"Entity '{entityType.Name}' with id '{id}' not found.");
            }

            return item;
        }
    }
}