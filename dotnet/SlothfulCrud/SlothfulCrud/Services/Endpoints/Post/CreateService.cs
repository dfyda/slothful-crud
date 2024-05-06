﻿using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Extensions;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Services.Endpoints.Post
{
    public class CreateService<TEntity, TContext> : ICreateService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly ICreateConstructorBehavior _createConstructorBehavior;
        private readonly IEntityConfigurationProvider _configurationProvider;
        private TContext DbContext { get; }
        
        public CreateService(
            TContext dbContext,
            ICreateConstructorBehavior createConstructorBehavior,
            IEntityConfigurationProvider configurationProvider)
        {
            DbContext = dbContext;
            _createConstructorBehavior = createConstructorBehavior;
            _configurationProvider = configurationProvider;
        }
        
        public object Create(object id, dynamic command, IServiceScope serviceScope)
        {
            var constructor = _createConstructorBehavior.GetConstructorInfo(typeof(TEntity));
            if (constructor is null)
            {
                throw new ConfigurationException($"Entity '{typeof(TEntity).Name}' must have a constructor.");
            }
            
            var constructorArgs = constructor.GetParameters().ToArray()
                .Select(param => param.GetDomainMethodParam((object)command, DbContext))
                .ToArray();

            constructorArgs[0] = id;
            
            var item = (TEntity)constructor.Invoke(constructorArgs);
            
            if (_configurationProvider.GetConfiguration(typeof(TEntity)).HasValidation)
            {
                SlothfulTypesProvider.GetConcreteValidator<TEntity>(serviceScope).ValidateAndThrow(item);
            }
            
            DbContext.Set<TEntity>().Add(item);
            DbContext.SaveChanges();
            
            return id;
        }
    }
}