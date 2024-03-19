using System.Linq.Expressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Providers;

namespace SlothfulCrud.DynamicTypes
{
    public class DynamicLambdaBuilder
    {
        public static LambdaExpression CreateLambdaForDynamicType(
            Type dynamicType,
            WebApplication webApplication,
            Type dbContextType,
            Type entityType)
        {
            var service = GetService(webApplication, dbContextType, entityType);
            var createMethod = service.GetType().GetMethod("Create", new[] { typeof(Guid) });
            
            if (createMethod is null)
            {
                throw new InvalidOperationException("Method 'Create' not found on the provided service.");
            }
            
            var commandParam = Expression.Parameter(dynamicType, "command");
            var idVariable = Expression.Variable(typeof(Guid), "id");

            var body = Expression.Block(
                new[] { idVariable },
                
                Expression.Assign(idVariable, Expression.Call(typeof(Guid).GetMethod("NewGuid"))),
                
                Expression.Call(Expression.Constant(service), createMethod, idVariable),
                
                Expression.Call(
                    typeof(Results).GetMethod("Created", new[] { typeof(string), typeof(object) }),
                    Expression.Constant($"/{dynamicType.Name}s/"),
                    Expression.Convert(idVariable, typeof(object)))
            );

            return Expression.Lambda(body, "test", new List<ParameterExpression>() { commandParam });
        }
        
        private static dynamic GetService(WebApplication webApplication, Type dbContextType,
            Type entityType)
        {
            var serviceType = GetScope(webApplication, dbContextType, entityType,
                out var scope);
            var service = scope.ServiceProvider.GetService(serviceType) as dynamic;
            return service;
        }
        
        private static Type GetScope(WebApplication webApplication, Type dbContextType,
            Type entityType, out IServiceScope scope)
        {
            var serviceType = SlothfulTypesProvider.GetConcreteOperationService(entityType, dbContextType);
            scope = webApplication.Services.CreateScope();
            return serviceType;
        }
    }
}