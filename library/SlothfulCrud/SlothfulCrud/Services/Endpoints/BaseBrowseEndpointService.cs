using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Extensions;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Services.Endpoints
{
    internal abstract class BaseBrowseEndpointService<TEntity, TContext> : BaseEndpointService<TEntity>
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly IEntityConfigurationProvider _configurationProvider;
        
        protected BaseBrowseEndpointService(
            IEntityConfigurationProvider configurationProvider) : base(configurationProvider)
        {
            _configurationProvider = configurationProvider;
        }

        protected string GetEntitySortProperty(Type entityType)
        {
            return _configurationProvider.GetConfiguration(entityType).SortProperty;
        }
        
        protected int CalculateSkip(ushort page, dynamic query)
        {
            if (page == 0)
            {
                return 0;
            }
            return query.Rows * (page - 1);
        }

        protected void CheckSortByParameters(dynamic query)
        {
            if (typeof(TEntity).GetProperties().All(x => x.Name != query.SortBy.ToString()))
            {
                throw new ConfigurationException($"Property '{query.SortBy}' not found on type '{typeof(TEntity).Name}'.");
            }
            
            var sortDirection = (string)query.SortDirection;
            if (!sortDirection.Equals("asc", StringComparison.CurrentCultureIgnoreCase)
                && !sortDirection.Equals("desc", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new ConfigurationException($"Sort direction '{query.SortDirection}' is invalid. Must be 'asc' or 'desc'.");
            }
        }
        
        protected IQueryable<TEntity> SortQuery(dynamic query, IQueryable<TEntity> queryObject)
        {
            if (query.SortBy is null)
            {
                return queryObject;
            }
            
            CheckSortByParameters(query);
                
            var sortBy = (string)query.SortBy;
            var sortProperty = typeof(TEntity).GetProperties().First(x => x.Name == sortBy);
            if (sortProperty.PropertyType.IsClass && sortProperty.PropertyType != typeof(string))
            {
                var nestedSortProperty = GetEntitySortProperty(sortProperty.PropertyType);
                queryObject = queryObject.OrderByNestedProperty($"{sortBy}.{nestedSortProperty}", (string)query.SortDirection == "asc");
                return queryObject;
            }
            
            var sortDirection = (string)query.SortDirection;
            
            queryObject = sortDirection.Equals("asc", StringComparison.CurrentCultureIgnoreCase)
                ? queryObject.OrderBy(x => EF.Property<object>(x, sortBy))
                : queryObject.OrderByDescending(x => EF.Property<object>(x, sortBy));

            return queryObject;
        }
    }
}