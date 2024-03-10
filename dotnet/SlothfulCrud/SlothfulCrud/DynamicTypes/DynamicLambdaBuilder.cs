using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;

namespace SlothfulCrud.DynamicTypes
{
    public class DynamicLambdaBuilder
    {
        public static LambdaExpression CreateLambdaForDynamicType(
            Type dynamicType,
            dynamic service)
        {
            var body = Expression.Block(
                Expression.Variable(dynamicType, "command"),
                
                // var id = Guid.NewGuid();
                Expression.Assign(Expression.Variable(typeof(Guid), "id"),
                    Expression.Call(typeof(Guid).GetMethod("NewGuid"))),
            
                // service.Create(id);
                Expression.Call(service.GetMethod("Create", new[] { typeof(Guid) }),
                    Expression.Variable(typeof(Guid), "id")),
            
                // return Results.Created($"/{entityType.Name}s/{id}", id);
                Expression.Return(Expression.Label(),
                    Expression.Call(
                        typeof(Results).GetMethod("Created", new[] { typeof(string), typeof(object) }),
                        Expression.Constant($"/{dynamicType.Name}s/"),
                        Expression.Variable(typeof(Guid), "id")))
            );

            return Expression.Lambda(body, service, Expression.Parameter(dynamicType, "command"));
        }
    }
}