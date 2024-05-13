using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Extensions;
using SlothfulCrud.Providers;
using SlothfulCrud.Types;
using SlothfulCrud.Types.Dto;

namespace SlothfulCrud.Services.Endpoints.Get
{
    public class BrowseSelectableService<TEntity, TContext> : IBrowseSelectableService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly IEntityConfigurationProvider _configurationProvider;
        private TContext DbContext { get; }
        
        public BrowseSelectableService(TContext dbContext, IEntityConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
            DbContext = dbContext;
        }
        
        public PagedResults<BaseEntityDto> Browse(ushort page, dynamic query)
        {
            var baseQuery = DbContext.Set<TEntity>().AsQueryable();

            var resultQuery = FilterQuery(query.Search, baseQuery) as IQueryable<TEntity>;
            resultQuery = SortQuery(query, resultQuery);

            resultQuery = resultQuery
                .Take((int)query.Rows)
                .Skip((int)query.Skip);

            var data = GetBaseEntities(resultQuery);
            var total = baseQuery.Count();
            
            return new PagedResults<BaseEntityDto>(query.Skip, total, page, data.ToList());
        }

        private IQueryable<TEntity> SortQuery(dynamic query, IQueryable<TEntity> queryObject)
        {
            if (query.SortBy is null)
            {
                return queryObject;
            }
            
            var sortBy = _configurationProvider.GetConfiguration(typeof(TEntity)).SortProperty;
            
            queryObject = ((string)query.SortDirection).ToLower() == "asc"
                ? queryObject.OrderBy(x => EF.Property<string>(x, sortBy))
                : queryObject.OrderByDescending(x => EF.Property<string>(x, sortBy));

            return queryObject;
        }

        private IQueryable<TEntity> FilterQuery(string search, IQueryable<TEntity> queryObject)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var filterProperty = _configurationProvider.GetConfiguration(typeof(TEntity)).FilterProperty;
                queryObject = queryObject.Where(x => EF.Property<string>(x, filterProperty).Contains(search));
            }

            return queryObject;
        }
        
        private IEnumerable<BaseEntityDto> GetBaseEntities(IQueryable<TEntity> resultQuery)
        {
            var configuration = _configurationProvider.GetConfiguration(typeof(TEntity));
            var items = resultQuery.ToList();
            foreach (var item in items)
            {
                var propertyKeyValue = item.GetKeyPropertyValue(configuration.KeyProperty);
                yield return new BaseEntityDto
                {
                    Id = propertyKeyValue,
                    DisplayName = item.DisplayName
                };
            }
        }
    }
}