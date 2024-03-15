﻿using System.Dynamic;
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
            return new PagedResults<T>(30, 0, 0, new List<T>());
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