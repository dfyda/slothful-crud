using SlothfulCrud.Domain;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Configurations.Methods
{
    public class SlothfulDeleteEndpointConfigurationBuilder<T> : SlothfulEndpointConfigurationBuilder<T> where T : class, ISlothfulEntity
    {
        public SlothfulDeleteEndpointConfigurationBuilder(EndpointConfiguration configuration)
            : base(configuration)
        {
        }
    }
}