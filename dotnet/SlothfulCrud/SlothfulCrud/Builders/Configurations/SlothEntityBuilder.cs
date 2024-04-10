using SlothfulCrud.Builders.Configurations.Methods;
using SlothfulCrud.Domain;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Configurations
{
    public class SlothEntityBuilder<TEntity> where TEntity : class, ISlothfulEntity
    {
        public SlothfulGetEndpointConfigurationBuilder<TEntity> GetEndpoint => new();
        public SlothfulBrowseEndpointConfigurationBuilder<TEntity> BrowseEndpoint => new();
        public SlothfulCreateEndpointConfigurationBuilder<TEntity> CreateEndpoint => new();
        public SlothfulUpdateEndpointConfigurationBuilder<TEntity> UpdateEndpoint => new();
        public SlothfulDeleteEndpointConfigurationBuilder<TEntity> DeleteEndpoint => new();
        protected GlobalConfiguration GlobalConfiguration { get; set; }

        public SlothEntityBuilder()
        {
            GlobalConfiguration = new GlobalConfiguration();
        }
        
        public virtual SlothEntityBuilder<TEntity> AllowAnonymous()
        {
            GlobalConfiguration.SetIsAuthorizationEnable(false);
            return this;
        }
        
        public virtual SlothEntityBuilder<TEntity> RequireAuthorization(params string[] policyNames)
        {
            GlobalConfiguration.SetIsAuthorizationEnable(true);
            GlobalConfiguration.SetPolicyNames(policyNames);
            return this;
        }
        
        public virtual SlothEntityBuilder<TEntity> HasSortProperty(string sortProperty)
        {
            GlobalConfiguration.SetSortProperty(sortProperty);
            return this;
        }
        
        public virtual SlothEntityBuilder<TEntity> ExposeAllNestedProperties(bool expose = true)
        {
            GlobalConfiguration.SetExposeAllNestedProperties(expose);
            return this;
        }
        
        public EndpointsConfiguration Build()
        {
            var item = new EndpointsConfiguration(
                GetEndpoint.GetConfiguration);
            return item;
        }
    }
}