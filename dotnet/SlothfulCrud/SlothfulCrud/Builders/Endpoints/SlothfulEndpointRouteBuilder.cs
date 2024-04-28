using SlothfulCrud.Builders.Configurations;
using SlothfulCrud.Builders.Endpoints.Methods;
using SlothfulCrud.Builders.Endpoints.Parameters;
using SlothfulCrud.Domain;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Endpoints
{
    public class SlothfulEndpointRouteBuilder<TEntity> where TEntity : class, ISlothfulEntity
    {
        private readonly SlothEntityBuilder<TEntity> _configurationBuilder;
        protected SlothfulBuilderParams BuilderParams { get; set; }
        protected IDictionary<string, Type> GeneratedDynamicTypes { get; set; }
        public EndpointsConfiguration EndpointsConfiguration { get; protected set; }
        public SlothfulGetEndpointBuilder<TEntity> GetEndpoint 
            => new(BuilderParams, EndpointsConfiguration, GeneratedDynamicTypes, _configurationBuilder);
        public SlothfulBrowseEndpointBuilder<TEntity> BrowseEndpoint
            => new(BuilderParams, EndpointsConfiguration, GeneratedDynamicTypes, _configurationBuilder);
        public SlothfulBrowseSelectableEndpointBuilder<TEntity> BrowseSelectableEndpoint
            => new(BuilderParams, EndpointsConfiguration, GeneratedDynamicTypes, _configurationBuilder);
        public SlothfulDeleteEndpointBuilder<TEntity> DeleteEndpoint
            => new(BuilderParams, EndpointsConfiguration, GeneratedDynamicTypes, _configurationBuilder);
        public SlothfulUpdateEndpointBuilder<TEntity> UpdateEndpoint
            => new(BuilderParams, EndpointsConfiguration, GeneratedDynamicTypes, _configurationBuilder);
        public SlothfulCreateEndpointBuilder<TEntity> CreateEndpoint
            => new(BuilderParams, EndpointsConfiguration, GeneratedDynamicTypes, _configurationBuilder);

        public SlothfulEndpointRouteBuilder(SlothfulBuilderParams builderParams, SlothEntityBuilder<TEntity> configurationBuilder)
        {
            _configurationBuilder = configurationBuilder;
            GeneratedDynamicTypes = new Dictionary<string, Type>();
            BuilderParams = builderParams;
            EndpointsConfiguration = configurationBuilder.Build();
        }
    }
}