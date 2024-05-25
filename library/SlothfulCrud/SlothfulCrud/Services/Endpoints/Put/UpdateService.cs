﻿using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Extensions;
using SlothfulCrud.Providers;
using SlothfulCrud.Services.Endpoints.Get;
using FluentValidation;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Services.Endpoints.Put
{
    public class UpdateService<TEntity, TContext> : IUpdateService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly IGetService<TEntity, TContext> _getService;
        private readonly IEntityConfigurationProvider _configurationProvider;
        private TContext DbContext { get; }
        
        public UpdateService(
            TContext dbContext,
            IGetService<TEntity, TContext> getService,
            IEntityConfigurationProvider configurationProvider)
        {
            DbContext = dbContext;
            _getService = getService;
            _configurationProvider = configurationProvider;
        }
        
        public void Update(object keyProperty, dynamic command, IServiceScope serviceScope)
        {
            CheckEntityKey(typeof(TEntity), keyProperty);
            
            var updateMethod = GetModifyMethod();
            if (updateMethod is null)
            {
                throw new ConfigurationException($"Entity '{typeof(TEntity).Name}' must have a method named 'Update'.");
            }
            
            var item = _getService.Get(keyProperty);
            
            ModifyEntity(command, updateMethod, item);

            if (_configurationProvider.GetConfiguration(typeof(TEntity)).HasValidation)
            {
                SlothfulTypesProvider.GetConcreteValidator<TEntity>(serviceScope).ValidateAndThrow(item);
            }
            
            DbContext.SaveChanges();
        }

        private void ModifyEntity(dynamic command, MethodInfo updateMethod, TEntity item)
        {
            var methodArgs = updateMethod.GetParameters()
                .Select(param => param.GetDomainMethodParam((object)command, DbContext))
                .ToArray();

            updateMethod.Invoke(item, methodArgs);
        }

        private MethodInfo GetModifyMethod()
        {
            var methodName = _configurationProvider.GetConfiguration(typeof(TEntity)).UpdateMethod;
            return typeof(TEntity).GetMethod(methodName);
        }
        
        private void CheckEntityKey(Type type, object keyProperty)
        {
            var _entityConfiguration = _configurationProvider.GetConfiguration(typeof(TEntity));
            if (_entityConfiguration is null)
            {
                throw new ConfigurationException($"Entity '{typeof(TEntity)}' has no configuration.");
            }

            if (keyProperty is null)
            {
                throw new ConfigurationException($"Parameter '{nameof(keyProperty)}' cannot be null.");
            }
            
            if (type.GetProperty(_entityConfiguration.KeyProperty) is null)
            {
                throw new ConfigurationException($"Entity '{typeof(TEntity)}' must have a property named '{_entityConfiguration.KeyProperty}'");
            };

            if (keyProperty.GetType() != _entityConfiguration.KeyPropertyType)
            {
                throw new ConfigurationException($"Entity '{typeof(TEntity)}' key property '{_entityConfiguration.KeyProperty}' must be of type '{_entityConfiguration.KeyPropertyType}'");
            }
        }
    }
}