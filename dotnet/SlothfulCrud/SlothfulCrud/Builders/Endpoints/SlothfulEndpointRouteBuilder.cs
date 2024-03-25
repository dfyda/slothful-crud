using Microsoft.AspNetCore.Builder;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Builders.Endpoints.Behaviors.ModifyMethod;
using SlothfulCrud.Builders.Endpoints.Methods;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Builders.Endpoints
{
    public class SlothfulEndpointRouteBuilder
    {
        private readonly WebApplication _webApplication;
        private readonly Type _dbContextType;
        private readonly IApiSegmentProvider _apiSegmentProvider;
        private readonly ICreateConstructorBehavior _createConstructorBehavior;
        private readonly IModifyMethodBehavior _modifyMethodBehavior;
        public readonly SlothfulGetEndpointBuilder GetEndpoint;
        public readonly SlothfulBrowseEndpointBuilder BrowseEndpoint;
        public readonly SlothfulDeleteEndpointBuilder DeleteEndpoint;
        public readonly SlothfulUpdateEndpointBuilder UpdateEndpoint;
        public readonly SlothfulCreateEndpointBuilder CreateEndpoint;

        public SlothfulEndpointRouteBuilder(
            WebApplication webApplication,
            Type dbContextType,
            IApiSegmentProvider apiSegmentProvider,
            ICreateConstructorBehavior createConstructorBehavior,
            IModifyMethodBehavior modifyMethodBehavior)
        {
            _webApplication = webApplication;
            _dbContextType = dbContextType;
            _apiSegmentProvider = apiSegmentProvider;
            _createConstructorBehavior = createConstructorBehavior;
            _modifyMethodBehavior = modifyMethodBehavior;
            GetEndpoint = new SlothfulGetEndpointBuilder(webApplication, dbContextType, apiSegmentProvider);
            BrowseEndpoint = new SlothfulBrowseEndpointBuilder(webApplication, dbContextType, apiSegmentProvider);
            DeleteEndpoint = new SlothfulDeleteEndpointBuilder(webApplication, dbContextType, apiSegmentProvider);
            UpdateEndpoint = new SlothfulUpdateEndpointBuilder(webApplication, dbContextType, apiSegmentProvider, modifyMethodBehavior);
            CreateEndpoint = new SlothfulCreateEndpointBuilder(webApplication, dbContextType, apiSegmentProvider, createConstructorBehavior);
        }
    }
}