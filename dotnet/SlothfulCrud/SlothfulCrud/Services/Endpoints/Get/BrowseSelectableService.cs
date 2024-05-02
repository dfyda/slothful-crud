using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Providers;
using SlothfulCrud.Types;
using SlothfulCrud.Types.Dto;

namespace SlothfulCrud.Services.Endpoints.Get
{
    public class BrowseSelectableService<T, TContext> : IBrowseSelectableService<T, TContext> 
        where T : class, ISlothfulEntity, new() 
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
            var baseQuery = DbContext.Set<T>().AsQueryable();

            var resultQuery = FilterQuery(query.Search, baseQuery) as IQueryable<T>;
            resultQuery = SortQuery(query, resultQuery);

            resultQuery = resultQuery
                .Take((int)query.Rows)
                .Skip((int)query.Skip);

            var data = GetBaseEntities(resultQuery);
            var total = baseQuery.Count();
            
            return new PagedResults<BaseEntityDto>(query.Skip, total, page, data.ToList());
        }

        private IQueryable<T> SortQuery(dynamic query, IQueryable<T> queryObject)
        {
            var sortBy = _configurationProvider.GetConfiguration(typeof(T)).SortProperty;
            
            queryObject = ((string)query.SortDirection).ToLower() == "asc"
                ? queryObject.OrderBy(x => EF.Property<string>(x, sortBy))
                : queryObject.OrderByDescending(x => EF.Property<string>(x, sortBy));

            return queryObject;
        }

        private IQueryable<T> FilterQuery(string search, IQueryable<T> queryObject)
        {
            var filterProperty = _configurationProvider.GetConfiguration(typeof(T)).FilterProperty;
            return queryObject.Where(x => EF.Property<string>(x, filterProperty).Contains(search));
        }
        
        private IEnumerable<BaseEntityDto> GetBaseEntities(IQueryable<T> resultQuery)
        {
            var items = resultQuery.ToList();
            foreach (var item in items)
            {
                yield return new BaseEntityDto
                {
                    Id = item.Id,
                    DisplayName = item.DisplayName
                };
            }
        }
    }
}