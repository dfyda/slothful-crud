using SlothfulCrud.Builders.Configurations.Methods;
using SlothfulCrud.Domain;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Configurations
{
    public class SlothEntityBuilder<T> where T : class, ISlothfulEntity
    {
        public SlothfulGetEndpointConfigurationBuilder<T> GetEndpoint => new();
        public SlothfulBrowseEndpointConfigurationBuilder<T> BrowseEndpoint => new();
        public SlothfulCreateEndpointConfigurationBuilder<T> CreateEndpoint => new();
        public SlothfulUpdateEndpointConfigurationBuilder<T> UpdateEndpoint => new();
        public SlothfulDeleteEndpointConfigurationBuilder<T> DeleteEndpoint => new();
        protected GlobalConfiguration GlobalConfiguration { get; set; }
        
        public virtual SlothEntityBuilder<T> AllowAnonymous()
        {
            GlobalConfiguration.SetIsAuthorizationEnable(false);
            return this;
        }
        
        public virtual SlothEntityBuilder<T> RequireAuthorization(params string[] policyNames)
        {
            GlobalConfiguration.SetIsAuthorizationEnable(true);
            GlobalConfiguration.SetPolicyNames(policyNames);
            return this;
        }
        
        public virtual SlothEntityBuilder<T> HasSortProperty(string sortProperty)
        {
            GlobalConfiguration.SetSortProperty(sortProperty);
            return this;
        }
        
        public virtual SlothEntityBuilder<T> ExposeAllNestedProperties(bool expose = true)
        {
            GlobalConfiguration.SetExposeAllNestedProperties(expose);
            return this;
        }
    }
}