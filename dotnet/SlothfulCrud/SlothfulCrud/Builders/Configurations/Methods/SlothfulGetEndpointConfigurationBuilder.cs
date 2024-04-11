using SlothfulCrud.Domain;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Configurations.Methods
{
    public class SlothfulGetEndpointConfigurationBuilder<T> : SlothfulEndpointConfigurationBuilder<T> where T : class, ISlothfulEntity
    {
        public EndpointConfiguration GetConfiguration { get; protected set; } = new EndpointConfiguration();
        
        public SlothfulEndpointConfigurationBuilder<T> HasEndpoint(bool hasEndpoint = true)
        {
            GetConfiguration.SetIsEnable(hasEndpoint);
            return this;
        }
        
        public override SlothfulEndpointConfigurationBuilder<T> AllowAnonymous()
        {
            GetConfiguration.SetIsAuthorizationEnable(false);
            return this;
        }
        
        public override SlothfulEndpointConfigurationBuilder<T> RequireAuthorization(params string[] policyNames)
        {
            GetConfiguration.SetIsAuthorizationEnable(true);
            GetConfiguration.SetPolicyNames(policyNames);
            return this;
        }
        
        public override SlothfulEndpointConfigurationBuilder<T> HasSortProperty(string sortProperty)
        {
            GetConfiguration.SetSortProperty(sortProperty);
            return this;
        }
        
        public override SlothfulEndpointConfigurationBuilder<T> ExposeAllNestedProperties(bool expose = true)
        {
            GetConfiguration.SetExposeAllNestedProperties(expose);
            return this;
        }
    }
}