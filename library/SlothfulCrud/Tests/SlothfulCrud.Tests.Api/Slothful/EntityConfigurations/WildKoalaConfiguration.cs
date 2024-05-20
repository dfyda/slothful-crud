using SlothfulCrud.Builders.Configurations;
using SlothfulCrud.Tests.Api.Domain;

namespace SlothfulCrud.Tests.Api.Slothful.EntityConfigurations
{
    public class WildKoalaConfiguration : ISlothEntityConfiguration<WildKoala>
    {
        public void Configure(SlothEntityBuilder<WildKoala> builder)
        {
            builder.GetEndpoint
                .AllowAnonymous();
                
            builder.BrowseEndpoint
                .ExposeAllNestedProperties()
                .AllowAnonymous();
        }
    }
}