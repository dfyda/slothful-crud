using SlothfulCrud.Builders.Configurations;
using SlothfulCrud.Builders.Endpoints.Parameters;
using SlothfulCrud.Domain;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Endpoints.Methods
{
    public class SlothfulBrowseSelectableEndpointBuilder<TEntity> : SlothfulMethodEndpointRouteBuilder<TEntity> 
        where TEntity : class, ISlothfulEntity
    {
        public SlothfulBrowseSelectableEndpointBuilder(
            SlothfulBuilderParams builderParams,
            EndpointsConfiguration endpointsConfiguration,
            IDictionary<string, Type> generatedDynamicTypes,
            SlothEntityBuilder<TEntity> configurationBuilder
        ) : base(builderParams, endpointsConfiguration, generatedDynamicTypes, configurationBuilder)
        {
        }
        
        public SlothfulBrowseSelectableEndpointBuilder<TEntity> Map()
        {
            if (!EndpointsConfiguration.BrowseSelectable.IsEnable) 
                return this;
            
            // TODO: Implement the rest of the method
            
            return this;
        }
    }
}