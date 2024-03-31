﻿using SlothfulCrud.Builders.Endpoints.Methods;
using SlothfulCrud.Builders.Endpoints.Parameters;

namespace SlothfulCrud.Builders.Endpoints
{
    public class SlothfulEndpointRouteBuilder
    {
        protected SlothfulBuilderParams BuilderParams { get; set; }
        protected static IDictionary<string, Type> GeneratedDynamicTypes { get; set; } = new Dictionary<string, Type>();
        public SlothfulGetEndpointBuilder GetEndpoint => new(BuilderParams);
        public SlothfulBrowseEndpointBuilder BrowseEndpoint => new(BuilderParams);
        public SlothfulDeleteEndpointBuilder DeleteEndpoint => new(BuilderParams);
        public SlothfulUpdateEndpointBuilder UpdateEndpoint => new(BuilderParams);
        public SlothfulCreateEndpointBuilder CreateEndpoint => new(BuilderParams);

        public SlothfulEndpointRouteBuilder(SlothfulBuilderParams builderParams)
        {
            BuilderParams = builderParams;
        }
    }
}