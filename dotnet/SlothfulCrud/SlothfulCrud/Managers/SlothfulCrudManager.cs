using System.Reflection;
using Microsoft.AspNetCore.Builder;
using SlothfulCrud.Builders;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Managers
{
    public class SlothfulCrudManager : ISlothfulCrudManager
    {
        private readonly ISlothfulEndpointRouteBuilder _endpointRouteBuilder;

        public SlothfulCrudManager(ISlothfulEndpointRouteBuilder endpointRouteBuilder)
        {
            _endpointRouteBuilder = endpointRouteBuilder;
        }
        
        public WebApplication Register(WebApplication webApplication, Type dbContextType, Assembly executingAssembly)
        {
            var entityTypes = SlothfulTypesProvider.GetSlothfulEntityTypes(executingAssembly);
            foreach (var entityType in entityTypes)
            {
                _endpointRouteBuilder.MapEndpoints(webApplication, dbContextType, entityType);
            }

            return webApplication;
        }
    }
}