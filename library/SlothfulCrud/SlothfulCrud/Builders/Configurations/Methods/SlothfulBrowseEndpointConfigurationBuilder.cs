using SlothfulCrud.Domain;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Builders.Configurations.Methods
{
    public class SlothfulBrowseEndpointConfigurationBuilder<T> : SlothfulEndpointConfigurationBuilder<T> where T : class, ISlothfulEntity
    {
        public SlothfulBrowseEndpointConfigurationBuilder(EndpointConfiguration configuration)
            : base(configuration)
        {
        }
    }
}