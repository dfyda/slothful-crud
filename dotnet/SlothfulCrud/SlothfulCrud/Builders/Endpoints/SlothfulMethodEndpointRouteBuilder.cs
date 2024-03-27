using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SlothfulCrud.Builders.Endpoints.Parameters;

namespace SlothfulCrud.Builders.Endpoints
{
    public abstract class SlothfulMethodEndpointRouteBuilder : SlothfulEndpointRouteBuilder
    {
        protected RouteHandlerBuilder ConventionBuilder { get; set; }
        
        protected SlothfulMethodEndpointRouteBuilder(SlothfulBuilderParams builderParams) : base(builderParams)
        {
            BuilderParams = builderParams;
        }
        
        public SlothfulMethodEndpointRouteBuilder Produces(int statusCode)
        {
            ConventionBuilder.Produces(statusCode);
            return this;
        }

        public SlothfulMethodEndpointRouteBuilder Produces(int statusCode, Type responseType)
        {
            ConventionBuilder.Produces(statusCode, responseType);
            return this;
        }

        public SlothfulMethodEndpointRouteBuilder Produces<TResponse>(
            int statusCode = StatusCodes.Status200OK,
            string contentType = null,
            params string[] additionalContentTypes)
        {
            ConventionBuilder.Produces(statusCode, typeof(TResponse), contentType, additionalContentTypes);
            return this;
        }
    }
}