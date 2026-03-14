using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Extensions;
using SlothfulCrud.Providers;
using SlothfulCrud.Types;
using SlothfulCrud.Types.Configurations;
using SlothfulCrud.Types.Dto;

namespace SlothfulCrud.Services.Endpoints.Get
{
    internal class BrowseSelectableService<TEntity, TContext> : BaseBrowseEndpointService<TEntity, TContext>, IBrowseSelectableService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private TContext DbContext { get; }
        
        public BrowseSelectableService(
            TContext dbContext,
            IEntityConfigurationProvider configurationProvider,
            SlothfulOptions options) : base(configurationProvider, options)
        {
            DbContext = dbContext;
        }
        
        public async Task<PagedResults<BaseEntityDto>> BrowseAsync(ushort page, dynamic query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }
            
            var baseQuery = DbContext.Set<TEntity>().AsNoTracking().AsQueryable();
            baseQuery = ApplyQueryCustomizer(baseQuery);

            var resultQuery = FilterQuery(query.Search, baseQuery) as IQueryable<TEntity>;
            resultQuery = SortQuery(query, resultQuery);

            var total = await resultQuery.CountAsync();
            var skip = CalculateSkip(page, query);
            var pagedQuery = resultQuery
                .Skip((int)skip)
                .Take((int)query.Rows);

            var data = await GetBaseEntitiesAsync(pagedQuery);
            
            return new PagedResults<BaseEntityDto>(skip, total, query.Rows, data);
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
        
        private async Task<List<BaseEntityDto>> GetBaseEntitiesAsync(IQueryable<TEntity> resultQuery)
        {
            var items = await resultQuery.ToListAsync();
            var result = new List<BaseEntityDto>(items.Count);
            foreach (var item in items)
            {
                var propertyKeyValue = item.GetKeyPropertyValue(EntityConfiguration.KeyProperty);
                result.Add(new BaseEntityDto
                {
                    Id = propertyKeyValue,
                    DisplayName = item.DisplayName
                });
            }
            return result;
        }
    }
}
