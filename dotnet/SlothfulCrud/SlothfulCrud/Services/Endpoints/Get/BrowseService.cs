using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Types;

namespace SlothfulCrud.Services.Endpoints.Get
{
    public class BrowseService<T, TContext> : IBrowseService<T, TContext> 
        where T : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private TContext DbContext { get; }
        
        public BrowseService(TContext dbContext)
        {
            DbContext = dbContext;
        }
        
        public PagedResults<T> Browse(ushort page, dynamic query)
        {
            var queryObject = DbContext.Set<T>().AsQueryable();
            var properties = query.GetType().GetProperties();

            // TO DO: Add configuration for custom filtering and sorting
            queryObject = FilterQuery(query, properties, queryObject);
            queryObject = SortQuery(query, queryObject);

            var totalQuery = queryObject;
            queryObject = queryObject
                .Take((int)query.Rows)
                .Skip((int)query.Skip);

            var data = queryObject.ToList();
            var total = totalQuery.Count();
            
            return new PagedResults<T>(query.Skip, total, page, data);
        }
        
        private static IQueryable<T> SortQuery(dynamic query, IQueryable<T> queryObject)
        {
            if (query.SortBy is null)
            {
                return queryObject;
            }
            
            if (typeof(T).GetProperties().All(x => x.Name != query.SortBy.ToString()))
            {
                throw new ConfigurationException($"Property '{query.SortBy}' not found on type '{typeof(T).Name}'.");
            }
                
            var sortBy = (string)query.SortBy;
            queryObject = ((string)query.SortDirection).ToLower() == "asc"
                ? queryObject.OrderBy(x => EF.Property<object>(x, sortBy))
                : queryObject.OrderByDescending(x => EF.Property<object>(x, sortBy));

            return queryObject;
        }

        private IQueryable<T> FilterQuery(dynamic query, dynamic properties, IQueryable<T> queryObject)
        {
            foreach (var propertyInfo in properties)
            {
                string propertyName = propertyInfo.Name;
                var propertyValue = (object)propertyInfo.GetValue(query);
                if (propertyValue is null || BrowseFields.Fields.ContainsKey(propertyName))
                {
                    continue;
                }
                
                var propertyType = propertyInfo.PropertyType;
                if (propertyType == typeof(string))
                {
                    queryObject = queryObject.Where(x => EF.Property<string>(x, propertyName).Contains((string)propertyValue));
                }
                else
                {
                    queryObject = FilterField(propertyName, propertyValue, queryObject);
                }
            }

            return queryObject;
        }
        
        private IQueryable<TDbSet> FilterField<TDbSet, TField>(string propertyName, TField propertyValue, IQueryable<TDbSet> queryObject)
        {
            return queryObject.Where(x => EF.Property<TField>(x, propertyName).Equals(propertyValue));
        }
    }
}