using SlothfulCrud.Domain;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Configurations.Methods
{
    public class SlothfulGetEndpointConfigurationBuilder<T> : SlothfulEndpointConfigurationBuilder<T> where T : class, ISlothfulEntity
    {
        public SlothfulGetEndpointConfigurationBuilder(EndpointConfiguration configuration)
            : base(configuration)
        {
        }
    }
}