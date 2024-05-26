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
    public class UpdateService<TEntity, TContext> : BaseEndpointService<TEntity>, IUpdateService<TEntity, TContext> 
        where TEntity : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly IGetService<TEntity, TContext> _getService;
        private TContext DbContext { get; }
        
        public UpdateService(
            TContext dbContext,
            IGetService<TEntity, TContext> getService,
            IEntityConfigurationProvider configurationProvider) : base(configurationProvider)
        {
            DbContext = dbContext;
            _getService = getService;
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

            if (EntityConfiguration.HasValidation)
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
            var methodName = EntityConfiguration.UpdateMethod;
            return typeof(TEntity).GetMethod(methodName);
        }
    }
}