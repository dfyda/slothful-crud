using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Builders.Endpoints.Behaviors.ModifyMethod;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Extensions;
using SlothfulCrud.Types;

namespace SlothfulCrud.Services
{
    public class OperationService<T, TDbContext> : IOperationService<T, TDbContext> 
        where T : class, ISlothfulEntity, new() 
        where TDbContext : DbContext
    {
        private readonly ICreateConstructorBehavior _createConstructorBehavior;
        private readonly IModifyMethodBehavior _modifyMethodBehavior;
        private TDbContext DbContext { get; }
        
        public OperationService(
            TDbContext dbContext,
            ICreateConstructorBehavior createConstructorBehavior,
            IModifyMethodBehavior modifyMethodBehavior)
        {
            _createConstructorBehavior = createConstructorBehavior;
            _modifyMethodBehavior = modifyMethodBehavior;
            DbContext = dbContext;
        }
        
        // TO DO: Add configuration for entity id parameter type
        public T Get(Guid id)
        {
            CheckEntityKey(typeof(T));
            
            return DbContext.Set<T>()
                .FirstOrDefault(x => EF.Property<Guid>(x, "Id") == id)
                .OrFail($"{typeof(T)}NotFound", $"{typeof(T)} with id '{id}' not found.");  
        }
        
        public void Delete(Guid id)
        {
            var item = Get(id);
            
            DbContext.Set<T>().Remove(item);
            DbContext.SaveChanges();
        }

        public Guid Create(Guid id, dynamic command)
        {
            var constructor = _createConstructorBehavior.GetConstructorInfo(typeof(T));
            if (constructor is null)
            {
                throw new ConfigurationException($"Entity '{typeof(T).Name}' must have a constructor.");
            }
            
            object[] constructorArgs = constructor.GetParameters()
                .Select(param => ((object)command).GetProperties()[param.Name])
                .ToArray();

            constructorArgs[0] = id;
            
            T instanceOfT = (T)constructor.Invoke(constructorArgs);
            
            DbContext.Set<T>().Add(instanceOfT);
            DbContext.SaveChanges();
            
            return id;
        }

        public void Update(Guid id, dynamic command)
        {
            var updateMethod = _modifyMethodBehavior.GetModifyMethod(typeof(T));
            if (updateMethod is null)
            {
                throw new ConfigurationException($"Entity '{typeof(T).Name}' must have a method named 'Update'.");
            }
            
            var item = Get(id);
            
            object[] methodArgs = updateMethod.GetParameters()
                .Select(param => ((object)command).GetProperties()[param.Name])
                .ToArray();
            
            updateMethod.Invoke(item, methodArgs);
            DbContext.SaveChanges();
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

        private static IQueryable<T> FilterQuery(dynamic query, dynamic properties, IQueryable<T> queryObject)
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
        
        private static IQueryable<TDbSet> FilterField<TDbSet, TField>(string propertyName, TField propertyValue, IQueryable<TDbSet> queryObject)
        {
            return queryObject.Where(x => EF.Property<TField>(x, propertyName).Equals(propertyValue));
        }

        private void CheckEntityKey(Type type)
        {
            if (type.GetProperty("Id") is null)
            {
                throw new ConfigurationException("Entity must have a property named 'Id'");
            };
        }
    }
}