using SlothfulCrud.Builders.Configurations;
using SlothfulCrud.Builders.Endpoints.Methods;
using SlothfulCrud.Builders.Endpoints.Parameters;
using SlothfulCrud.Domain;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Endpoints
{
    public class SlothfulEndpointRouteBuilder<TEntity> where TEntity : class, ISlothfulEntity
    {
        private readonly SlothfulEndpointConfigurationBuilder<TEntity> _configurationBuilder;
        protected SlothfulBuilderParams BuilderParams { get; set; }
        public EndpointsConfiguration EndpointsConfiguration { get; protected set; }
        protected static IDictionary<string, Type> GeneratedDynamicTypes { get; set; } = new Dictionary<string, Type>();
        public SlothfulGetEndpointBuilder<TEntity> GetEndpoint => new(BuilderParams, _configurationBuilder);
        public SlothfulBrowseEndpointBuilder<TEntity> BrowseEndpoint => new(BuilderParams, _configurationBuilder);
        public SlothfulDeleteEndpointBuilder<TEntity> DeleteEndpoint => new(BuilderParams, _configurationBuilder);
        public SlothfulUpdateEndpointBuilder<TEntity> UpdateEndpoint => new(BuilderParams, _configurationBuilder);
        public SlothfulCreateEndpointBuilder<TEntity> CreateEndpoint => new(BuilderParams, _configurationBuilder);

        public SlothfulEndpointRouteBuilder(SlothfulBuilderParams builderParams, SlothfulEndpointConfigurationBuilder<TEntity> configurationBuilder)
        {
            _configurationBuilder = configurationBuilder;
            BuilderParams = builderParams;
            EndpointsConfiguration = configurationBuilder.Build();
        }
    }
}