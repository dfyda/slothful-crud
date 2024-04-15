using SlothfulCrud.Domain;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Configurations
{
    public class SlothfulEndpointConfigurationBuilder<T> : SlothEntityBuilder<T> where T : class, ISlothfulEntity
    {
        public EndpointConfiguration Configuration { get; protected set; }
        
        // TODO: public SlothfulEndpointConfigurationBuilder(bool isDefault = false)
        public SlothfulEndpointConfigurationBuilder(EndpointConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public SlothfulEndpointConfigurationBuilder<T> HasEndpoint(bool hasEndpoint = true)
        {
            Configuration.SetIsEnable(hasEndpoint);
            return this;
        }
        
        public override SlothfulEndpointConfigurationBuilder<T> AllowAnonymous()
        {
            Configuration.SetIsAuthorizationEnable(false);
            return this;
        }
        
        public override SlothfulEndpointConfigurationBuilder<T> RequireAuthorization(params string[] policyNames)
        {
            Configuration.SetIsAuthorizationEnable(true);
            Configuration.SetPolicyNames(policyNames);
            return this;
        }
        
        public override SlothfulEndpointConfigurationBuilder<T> ExposeAllNestedProperties(bool expose = true)
        {
            Configuration.SetExposeAllNestedProperties(expose);
            return this;
        }
    }   
}