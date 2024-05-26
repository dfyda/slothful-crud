using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Extensions;
using SlothfulCrud.Providers;
using SlothfulCrud.Types;

namespace SlothfulCrud.Services.Endpoints.Get
{
    public class BrowseService<TEntity, TContext> : BaseBrowseEndpointService<TEntity, TContext>, IBrowseService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private TContext DbContext { get; }
        
        public BrowseService(TContext dbContext, IEntityConfigurationProvider configurationProvider) : base(configurationProvider)
        {
            DbContext = dbContext;
        }
        
        public PagedResults<TEntity> Browse(ushort page, dynamic query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }
            
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

        private IQueryable<TEntity> FilterStringField(IQueryable<TEntity> queryObject, string propertyName, object propertyValue)
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