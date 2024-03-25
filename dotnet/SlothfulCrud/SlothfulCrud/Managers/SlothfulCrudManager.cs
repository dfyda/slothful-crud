using System.Reflection;
using Microsoft.AspNetCore.Builder;
using SlothfulCrud.Builders.Endpoints;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Builders.Endpoints.Behaviors.ModifyMethod;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Managers
{
    public class SlothfulCrudManager : ISlothfulCrudManager
    {
        private readonly IApiSegmentProvider _apiSegmentProvider;
        private readonly ICreateConstructorBehavior _createConstructorBehavior;
        private readonly IModifyMethodBehavior _modifyMethodBehavior;

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
            var entityTypes = SlothfulTypesProvider.GetSlothfulEntityTypes(executingAssembly);
            foreach (var entityType in entityTypes)
            {
                var builder = new SlothfulEndpointRouteBuilder(
                    webApplication,
                    dbContextType,
                    _apiSegmentProvider,
                    _createConstructorBehavior,
                    _modifyMethodBehavior);
                
                builder
                    .GetEndpoint.Map(entityType)
                    .BrowseEndpoint.Map(entityType)
                    .CreateEndpoint.Map(entityType)
                    .UpdateEndpoint.Map(entityType)
                    .DeleteEndpoint.Map(entityType);
            }

            return webApplication;
        }
    }
}