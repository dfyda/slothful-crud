using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Extensions;
using SlothfulCrud.Providers;
using SlothfulCrud.Types;
using System.Reflection;

namespace SlothfulCrud.Services.Endpoints.Get
{
    internal class BrowseService<TEntity, TContext> : BaseBrowseEndpointService<TEntity, TContext>, IBrowseService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private TContext DbContext { get; }
        
        public BrowseService(TContext dbContext, IEntityConfigurationProvider configurationProvider) : base(configurationProvider)
        {
            DbContext = dbContext;
        }
        
        public async Task<PagedResults<TEntity>> BrowseAsync(ushort page, dynamic query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }
            
            var baseQuery = DbContext.Set<TEntity>()
                .AsNoTracking()
                .AsQueryable()
                .IncludeAllFirstLevelDependencies();
            var properties = ReflectionCache.GetProperties(query.GetType());

            var resultQuery = FilterQuery(query, properties, baseQuery) as IQueryable<TEntity>;
            resultQuery = SortQuery(query, resultQuery);

            var total = await resultQuery.CountAsync();
            var skip = CalculateSkip(page, query);
            var data = await resultQuery
                .Skip((int)skip)
                .Take((int)query.Rows)
                .ToListAsync();
            
            return new PagedResults<TEntity>(skip, total, query.Rows, data);
        }

        private IQueryable<TEntity> FilterQuery(dynamic query, PropertyInfo[] properties, IQueryable<TEntity> queryObject)
        {
            foreach (var propertyInfo in properties)
            {
                string propertyName = propertyInfo.Name;
                var propertyValue = propertyInfo.GetValue(query);
                if (propertyValue is null || BrowseFields.Fields.ContainsKey(propertyName))
                {
                    continue;
                }
                
                var propertyType = propertyInfo.PropertyType;
                
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