using SlothfulCrud.Domain;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Configurations
{
    public class SlothfulEndpointConfigurationBuilder<T> : SlothEntityBuilder<T> where T : class, ISlothfulEntity
    {
        protected EndpointConfiguration EndpointConfiguration { get; private set; }
        
        public SlothEntityBuilder<T> HasEndpoint(bool hasEndpoint = true)
        {
            EndpointConfiguration.SetIsEnable(hasEndpoint);
            return this;
        }
        
        public override SlothEntityBuilder<T> AllowAnonymous()
        {
            EndpointConfiguration.SetIsAuthorizationEnable(false);
            return this;
        }
        
        public override SlothEntityBuilder<T> RequireAuthorization(params string[] policyNames)
        {
            EndpointConfiguration.SetIsAuthorizationEnable(true);
            EndpointConfiguration.SetPolicyNames(policyNames);
            return this;
        }
        
        public override SlothEntityBuilder<T> HasSortProperty(string sortProperty)
        {
            EndpointConfiguration.SetSortProperty(sortProperty);
            return this;
        }
        
        public override SlothEntityBuilder<T> ExposeAllNestedProperties(bool expose = true)
        {
            EndpointConfiguration.SetExposeAllNestedProperties(expose);
            return this;
        }
    }   
}