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
    }
}