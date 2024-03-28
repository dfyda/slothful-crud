using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Builders.Endpoints.Behaviors.ModifyMethod;
using SlothfulCrud.Domain;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Extensions;
using SlothfulCrud.Services.Endpoints.Get;

namespace SlothfulCrud.Services.Endpoints.Put
{
    public class UpdateService<T, TContext> : IUpdateService<T, TContext> 
        where T : class, ISlothfulEntity, new() 
        where TContext : DbContext
    {
        private readonly IGetService<T, TContext> _getService;
        private readonly IModifyMethodBehavior _modifyMethodBehavior;
        private TContext DbContext { get; }
        
        public UpdateService(
            TContext dbContext,
            IGetService<T, TContext> getService,
            IModifyMethodBehavior modifyMethodBehavior)
        {
            DbContext = dbContext;
            _getService = getService;
            _modifyMethodBehavior = modifyMethodBehavior;
        }
        
        public void Update(Guid id, dynamic command)
        {
            var updateMethod = _modifyMethodBehavior.GetModifyMethod(typeof(T));
            if (updateMethod is null)
            {
                throw new ConfigurationException($"Entity '{typeof(T).Name}' must have a method named 'Update'.");
            }
            
            var item = _getService.Get(id);
            
            var methodArgs = updateMethod.GetParameters()
                .Select(param => ((object)command).GetProperties()[param.Name])
                .ToArray();
            
            updateMethod.Invoke(item, methodArgs);
            DbContext.SaveChanges();
        }
    }
}