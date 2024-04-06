using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace SlothfulCrud.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> IncludeAllFirstLevelDependencies<T>(this IQueryable<T> query) where T : class
        {
            var entityType = typeof(T);
            var properties = entityType.GetProperties()
                .Where(p => p.PropertyType.IsClass && p.PropertyType != typeof(string));

            foreach (var property in properties)
            {
                query = query.Include(property.Name);
            }

            return query;
        }
        
        public static IOrderedQueryable<TSource> OrderByNestedProperty<TSource>(
            this IQueryable<TSource> source,
            string propertyPath,
            bool descending = false)
        {
            if (source is null) throw new ArgumentNullException(nameof(source), "Source cannot be null.");
            if (string.IsNullOrWhiteSpace(propertyPath)) throw new ArgumentException("Property path cannot be null or whitespace.", nameof(propertyPath));

            var properties = propertyPath.Split('.');
            var parameter = Expression.Parameter(typeof(TSource), "x");

            Expression body = parameter;
            foreach (var property in properties)
            {
                body = Expression.PropertyOrField(body, property);
            }

            var lambdaType = typeof(Func<,>).MakeGenericType(typeof(TSource), body.Type);
            var lambda = Expression.Lambda(lambdaType, body, parameter);

            var methodName = descending ? "OrderByDescending" : "OrderBy";
            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TSource), body.Type);

            var resultExpression = Expression.Call(method, source.Expression, lambda);

            return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>(resultExpression);
        }
    }
}