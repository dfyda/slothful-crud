using SlothfulCrud.Builders.Configurations.Methods;
using SlothfulCrud.Domain;

namespace SlothfulCrud.Builders.Configurations
{
    public class SlothEntityBuilder<T> where T : class, ISlothfulEntity
    {
        public SlothfulGetEndpointConfigurationBuilder<T> GetEndpoint => new();
        public SlothfulBrowseEndpointConfigurationBuilder<T> BrowseEndpoint => new();
        public SlothfulCreateEndpointConfigurationBuilder<T> CreateEndpoint => new();
        public SlothfulUpdateEndpointConfigurationBuilder<T> UpdateEndpoint => new();
        public SlothfulDeleteEndpointConfigurationBuilder<T> DeleteEndpoint => new();
        
        public SlothEntityBuilder<T> AllowAnonymous()
        {
            return this;
        }
        
        public SlothEntityBuilder<T> RequireAuthorization(params string[] policyNames)
        {
            return this;
        }
        
        public SlothEntityBuilder<T> HasSortProperty(string propertyName)
        {
            return this;
        }
        
        public SlothEntityBuilder<T> ExposeAllNestedProperties(bool expose = true)
        {
            return this;
        }
    }
}