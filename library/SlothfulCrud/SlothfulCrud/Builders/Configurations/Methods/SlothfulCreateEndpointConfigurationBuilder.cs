using SlothfulCrud.Domain;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Configurations.Methods
{
    public class SlothfulCreateEndpointConfigurationBuilder<T> : SlothfulEndpointConfigurationBuilder<T> where T : class, ISlothfulEntity
    {
        public SlothfulCreateEndpointConfigurationBuilder(EndpointConfiguration configuration)
            : base(configuration)
        {
        }
    }
}