using System.Dynamic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
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
        private TDbContext DbContext { get; }
        
        public OperationService(TDbContext dbContext)
        {
            DbContext = dbContext;
        }
        
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
            ConstructorInfo constructor = typeof(T).GetConstructors()
                .FirstOrDefault(x => x.GetParameters().Length > 0);
            if (constructor is null)
            {
                throw new ConfigurationException($"Entity '{typeof(T).Name}' must have a constructor.");
            }
            
            object[] constructorArgs = constructor.GetParameters()
                .Select(param => GetProperties(command)[param.Name])
                .ToArray();

            constructorArgs[0] = id;
            
            T instanceOfT = (T)constructor.Invoke(constructorArgs);
            
            DbContext.Set<T>().Add(instanceOfT);
            DbContext.SaveChanges();
            
            return id;
        }

        public void Update(Guid id, dynamic command)
        {
            var updateMethod = typeof(T).GetMethod("Update");
            if (updateMethod is null)
            {
                throw new ConfigurationException($"Entity '{typeof(T).Name}' must have a method named 'Update'.");
            }
            
            var item = Get(id);
            
            object[] methodArgs = updateMethod.GetParameters()
                .Select(param => GetProperties(command)[param.Name])
                .ToArray();
            
            updateMethod.Invoke(item, methodArgs);
            DbContext.SaveChanges();
        }

        public PagedResults<T> Browse(ushort page, dynamic query)
        {
            var queryObject = DbContext.Set<T>().AsQueryable();
            var properties = query.GetType().GetProperties();

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
            }

            return queryObject;
        }

        private void CheckEntityKey(Type type)
        {
            if (type.GetProperty("Id") is null)
            {
                throw new ConfigurationException("Entity must have a property named 'Id'");
            };
        }
        
        static IDictionary<string, object> GetProperties(object obj)
        {
            if (obj is ExpandoObject expandoObject)
            {
                return expandoObject.ToDictionary(kv => kv.Key, kv => kv.Value);
            }

            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var propertyValues = new Dictionary<string, object>();

            foreach (var property in properties)
            {
                propertyValues[property.Name] = property.GetValue(obj);
            }

            return propertyValues;
        }
    }
}