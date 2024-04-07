using System.Reflection;
using Microsoft.AspNetCore.Builder;
using SlothfulCrud.Builders.Configurations;
using SlothfulCrud.Builders.Endpoints;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Builders.Endpoints.Behaviors.ModifyMethod;
using SlothfulCrud.Builders.Endpoints.Parameters;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Managers
{
    public class SlothfulCrudManager : ISlothfulCrudManager
    {
        private readonly IApiSegmentProvider _apiSegmentProvider;
        private readonly ICreateConstructorBehavior _createConstructorBehavior;
        private readonly IModifyMethodBehavior _modifyMethodBehavior;
        private readonly SlothConfigurationBuilder _slothConfigurationBuilder = new SlothConfigurationBuilder();

        public SlothfulCrudManager(
            IApiSegmentProvider apiSegmentProvider,
            ICreateConstructorBehavior createConstructorBehavior,
            IModifyMethodBehavior modifyMethodBehavior)
        {
            _apiSegmentProvider = apiSegmentProvider;
            _createConstructorBehavior = createConstructorBehavior;
            _modifyMethodBehavior = modifyMethodBehavior;
        }
        
        public WebApplication Register(WebApplication webApplication, Type dbContextType, Assembly executingAssembly)
        {
            _slothConfigurationBuilder.ApplyConfigurationsFromAssembly(executingAssembly);
            
            var entityTypes = SlothfulTypesProvider.GetSlothfulEntityTypes(executingAssembly);
            foreach (var entityType in entityTypes)
            {
                var parameters = new SlothfulBuilderParams(
                    webApplication,
                    dbContextType,
                    entityType,
                    _apiSegmentProvider,
                    _createConstructorBehavior,
                    _modifyMethodBehavior);
                var builder = new SlothfulEndpointRouteBuilder(parameters);
                
                builder
                    .GetEndpoint.Map()
                    .BrowseEndpoint.Map()
                    .CreateEndpoint.Map()
                    .UpdateEndpoint.Map()
                    .DeleteEndpoint.Map();
            }

            return webApplication;
        }
    }
}