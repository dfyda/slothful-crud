using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Extensions;
using SlothfulCrud.Providers;
using SlothfulCrud.Types;
using SlothfulCrud.Types.Dto;

namespace SlothfulCrud.Services.Endpoints.Get
{
    internal class BrowseSelectableService<TEntity, TContext> : BaseBrowseEndpointService<TEntity, TContext>, IBrowseSelectableService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private TContext DbContext { get; }
        
        public BrowseSelectableService(TContext dbContext, IEntityConfigurationProvider configurationProvider) : base(configurationProvider)
        {
            DbContext = dbContext;
        }
        
        public PagedResults<BaseEntityDto> Browse(ushort page, dynamic query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }
            
            var baseQuery = DbContext.Set<TEntity>().AsQueryable();

            var resultQuery = FilterQuery(query.Search, baseQuery) as IQueryable<TEntity>;
            resultQuery = SortQuery(query, resultQuery);

            var skip = CalculateSkip(page, query);
            resultQuery = resultQuery
                .Skip((int)skip)
                .Take((int)query.Rows);

            var data = GetBaseEntities(resultQuery);
            var total = baseQuery.Count();
            
            return new PagedResults<BaseEntityDto>(skip, total, page, data.ToList());
        }

        private IQueryable<TEntity> FilterQuery(string search, IQueryable<TEntity> queryObject)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var filterProperty = EntityConfiguration.FilterProperty;
                queryObject = queryObject.Where(x => EF.Property<string>(x, filterProperty).Contains(search));
            }

            return queryObject;
        }
        
        private IEnumerable<BaseEntityDto> GetBaseEntities(IQueryable<TEntity> resultQuery)
        {
            var items = resultQuery.ToList();
            foreach (var item in items)
            {
                var propertyKeyValue = item.GetKeyPropertyValue(EntityConfiguration.KeyProperty);
                yield return new BaseEntityDto
                {
                    Id = propertyKeyValue,
                    DisplayName = item.DisplayName
                };
            }
        }
    }
}