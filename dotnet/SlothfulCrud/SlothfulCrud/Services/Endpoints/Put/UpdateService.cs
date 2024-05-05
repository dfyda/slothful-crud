using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Extensions;
using SlothfulCrud.Providers;
using SlothfulCrud.Services.Endpoints.Get;
using FluentValidation;

namespace SlothfulCrud.Services.Endpoints.Put
{
    public class UpdateService<T, TKeyProperty, TContext> : IUpdateService<T, TKeyProperty, TContext> 
        where T : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly IGetService<T, TKeyProperty, TContext> _getService;
        private readonly IEntityConfigurationProvider _configurationProvider;
        private TContext DbContext { get; }
        
        public UpdateService(
            TContext dbContext,
            IGetService<T, TKeyProperty, TContext> getService,
            IEntityConfigurationProvider configurationProvider)
        {
            DbContext = dbContext;
            _getService = getService;
            _configurationProvider = configurationProvider;
        }
        
        public void Update(TKeyProperty id, dynamic command, IServiceScope serviceScope)
        {
            var updateMethod = GetModifyMethod();
            if (updateMethod is null)
            {
                throw new ConfigurationException($"Entity '{typeof(T).Name}' must have a method named 'Update'.");
            }
            
            var item = _getService.Get(id);
            
            ModifyEntity(command, updateMethod, item);

            if (_configurationProvider.GetConfiguration(typeof(T)).HasValidation)
            {
                SlothfulTypesProvider.GetConcreteValidator<T>(serviceScope).ValidateAndThrow(item);
            }
            
            DbContext.SaveChanges();
        }

        private void ModifyEntity(dynamic command, MethodInfo updateMethod, T item)
        {
            var methodArgs = updateMethod.GetParameters()
                .Select(param => param.GetDomainMethodParam((object)command, DbContext))
                .ToArray();

            updateMethod.Invoke(item, methodArgs);
        }

        private MethodInfo GetModifyMethod()
        {
            var methodName = _configurationProvider.GetConfiguration(typeof(T)).UpdateMethod;
            return typeof(T).GetMethod(methodName);
        }
    }
}