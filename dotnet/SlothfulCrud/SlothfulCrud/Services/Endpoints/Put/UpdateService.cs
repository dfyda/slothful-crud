using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Extensions;
using SlothfulCrud.Providers;
using SlothfulCrud.Services.Endpoints.Get;

namespace SlothfulCrud.Services.Endpoints.Put
{
    public class UpdateService<T, TContext> : IUpdateService<T, TContext> 
        where T : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly IGetService<T, TContext> _getService;
        private readonly IEntityConfigurationProvider _configurationProvider;
        private TContext DbContext { get; }
        
        public UpdateService(
            TContext dbContext,
            IGetService<T, TContext> getService,
            IEntityConfigurationProvider configurationProvider)
        {
            DbContext = dbContext;
            _getService = getService;
            _configurationProvider = configurationProvider;
        }
        
        public void Update(Guid id, dynamic command)
        {
            var updateMethod = GetModifyMethod();
            if (updateMethod is null)
            {
                throw new ConfigurationException($"Entity '{typeof(T).Name}' must have a method named 'Update'.");
            }
            
            var item = _getService.Get(id);
            
            var methodArgs = updateMethod.GetParameters()
                .Select(param => param.GetDomainMethodParam((object)command, DbContext))
                .ToArray();
            
            updateMethod.Invoke(item, methodArgs);
            DbContext.SaveChanges();
        }

        private MethodInfo GetModifyMethod()
        {
            var methodName = _configurationProvider.GetConfiguration(typeof(T)).UpdateMethod;
            return typeof(T).GetMethod(methodName);
        }
    }
}