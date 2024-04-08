using SlothfulCrud.Domain;

namespace SlothfulCrud.Builders.Configurations
{
    public class SlothfulEndpointConfigurationBuilder<T> : SlothEntityBuilder<T> where T : class, ISlothfulEntity
    {
        public SlothEntityBuilder<T> HasEndpoint(bool hasEndpoint = true)
        {
            return this;
        }
    }   
}