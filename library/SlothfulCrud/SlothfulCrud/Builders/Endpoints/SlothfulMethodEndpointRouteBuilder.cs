using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SlothfulCrud.Builders.Configurations;
using SlothfulCrud.Builders.Endpoints.Parameters;
using SlothfulCrud.Domain;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Endpoints
{
    public abstract class SlothfulMethodEndpointRouteBuilder<TEntity> : SlothfulEndpointRouteBuilder<TEntity> 
        where TEntity : class, ISlothfulEntity
    {
        protected RouteHandlerBuilder ConventionBuilder { get; set; }
        
        protected SlothfulMethodEndpointRouteBuilder(
            SlothfulBuilderParams builderParams,
            EndpointsConfiguration endpointsConfiguration,
            IDictionary<string, Type> generatedDynamicTypes,
            SlothEntityBuilder<TEntity> configurationBuilder)
            : base(builderParams, configurationBuilder)
        {
            BuilderParams = builderParams;
            EndpointsConfiguration = endpointsConfiguration;
            GeneratedDynamicTypes = generatedDynamicTypes;
        }
        
        public SlothfulMethodEndpointRouteBuilder<TEntity> Produces(int statusCode)
        {
            ConventionBuilder.Produces(statusCode);
            return this;
        }

        public SlothfulMethodEndpointRouteBuilder<TEntity> Produces(int statusCode, Type responseType)
        {
            ConventionBuilder.Produces(statusCode, responseType);
            return this;
        }

        public SlothfulMethodEndpointRouteBuilder<TEntity> Produces<TResponse>(
            int statusCode = StatusCodes.Status200OK,
            string contentType = null,
            params string[] additionalContentTypes)
        {
            ConventionBuilder.Produces(statusCode, typeof(TResponse), contentType, additionalContentTypes);
            return this;
        }
    }
}