using SlothfulCrud.Domain;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Configurations.Methods
{
    public class SlothfulUpdateEndpointConfigurationBuilder<T> : SlothfulEndpointConfigurationBuilder<T> where T : class, ISlothfulEntity
    {
        public SlothfulUpdateEndpointConfigurationBuilder(EndpointConfiguration configuration)
            : base(configuration)
        {
        }
    }
}