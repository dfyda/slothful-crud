using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Extensions;
using SlothfulCrud.Providers;
using SlothfulCrud.Types;

namespace SlothfulCrud.Services.Endpoints.Get
{
    public class BrowseService<TEntity, TContext> : IBrowseService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly IEntityConfigurationProvider _configurationProvider;
        private TContext DbContext { get; }
        
        public BrowseService(TContext dbContext, IEntityConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
            DbContext = dbContext;
        }
        
        public PagedResults<TEntity> Browse(ushort page, dynamic query)
        {
            var baseQuery = DbContext.Set<TEntity>().AsQueryable().IncludeAllFirstLevelDependencies();
            var properties = query.GetType().GetProperties();

            var resultQuery = FilterQuery(query, properties, baseQuery) as IQueryable<TEntity>;
            resultQuery = SortQuery(query, resultQuery);

            var totalQuery = baseQuery;
            var skip = CalculateSkip(page, query);
            resultQuery = resultQuery
                .Skip((int)skip)
                .Take((int)query.Rows);

            var data = resultQuery.ToList();
            var total = totalQuery.Count();
            
            return new PagedResults<TEntity>(skip, total, page, data);
        }

        private int CalculateSkip(ushort page, dynamic query)
        {
            if (page == 0)
            {
                return 0;
            }
            return query.Rows * (page - 1);
        }

        private IQueryable<TEntity> SortQuery(dynamic query, IQueryable<TEntity> queryObject)
        {
            if (query.SortBy is null)
            {
                return queryObject;
            }
            
            if (typeof(TEntity).GetProperties().All(x => x.Name != query.SortBy.ToString()))
            {
                throw new ConfigurationException($"Property '{query.SortBy}' not found on type '{typeof(TEntity).Name}'.");
            }
                
            var sortBy = (string)query.SortBy;
            var sortProperty = typeof(TEntity).GetProperties().First(x => x.Name == sortBy);
            if (sortProperty.PropertyType.IsClass && sortProperty.PropertyType != typeof(string))
            {
                var nestedSortProperty = _configurationProvider.GetConfiguration(sortProperty.PropertyType).SortProperty;
                queryObject = queryObject.OrderByNestedProperty($"{sortBy}.{nestedSortProperty}", (string)query.SortDirection == "asc");
                return queryObject;
            }
            
            queryObject = ((string)query.SortDirection).ToLower() == "asc"
                ? queryObject.OrderBy(x => EF.Property<object>(x, sortBy))
                : queryObject.OrderByDescending(x => EF.Property<object>(x, sortBy));

            return queryObject;
        }

        private IQueryable<TEntity> FilterQuery(dynamic query, dynamic properties, IQueryable<TEntity> queryObject)
        {
            foreach (var propertyInfo in properties)
            {
                string propertyName = propertyInfo.Name;
                var propertyValue = (object)propertyInfo.GetValue(query);
                if (propertyValue is null || BrowseFields.Fields.ContainsKey(propertyName))
                {
                    continue;
                }
                
                var propertyType = propertyInfo.PropertyType as Type;
                
                if (propertyType == typeof(string))
                {
                    queryObject = FilterStringField(queryObject, propertyName, propertyValue);
                } 
                else if (propertyType == typeof(DateTime?))
                {
                    queryObject = FilterDateField(queryObject, propertyName, propertyValue);
                }
                else
                {
                    queryObject = FilterField(propertyName, propertyValue, queryObject);
                }
            }

            return queryObject;
        }

        private static IQueryable<TEntity> FilterDateField(IQueryable<TEntity> queryObject, string propertyName, object propertyValue)
        {
            if (propertyName.EndsWith("From"))
                queryObject = queryObject.Where(x => EF.Property<DateTime?>(x, propertyName.Replace("From", "")) >= ((DateTime?)propertyValue));
            if (propertyName.EndsWith("To"))
                queryObject = queryObject.Where(x => EF.Property<DateTime?>(x, propertyName.Replace("To", "")) < ((DateTime?)propertyValue).Value.AddDays(1));
            return queryObject;
        }

        private static IQueryable<TEntity> FilterStringField(IQueryable<TEntity> queryObject, string propertyName, object propertyValue)
        {
            queryObject = queryObject.Where(x => EF.Property<string>(x, propertyName).Contains((string)propertyValue));
            return queryObject;
        }

        private IQueryable<TDbSet> FilterField<TDbSet, TField>(string propertyName, TField propertyValue, IQueryable<TDbSet> queryObject)
        {
            return queryObject.Where(x => EF.Property<TField>(x, propertyName).Equals(propertyValue));
        }
    }
}