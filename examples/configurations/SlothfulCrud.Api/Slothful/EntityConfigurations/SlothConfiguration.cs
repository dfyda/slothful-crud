using SlothfulCrud.Api.Domain;
using SlothfulCrud.Builders.Configurations;

namespace SlothfulCrud.Api.Slothful.EntityConfigurations
{
    public class SlothConfiguration : ISlothEntityConfiguration<Sloth>
    {
        public void Configure(SlothEntityBuilder<Sloth> builder)
        {
            builder
                .SetSortProperty(x => x.Name)
                .SetFilterProperty(x => x.Name)
                .SetUpdateMethodName("Update")
                .HasValidation();

            builder.GetEndpoint
                .HasEndpoint()
                .ExposeAllNestedProperties(false)
                .AllowAnonymous();

            builder.BrowseEndpoint
                .HasEndpoint()
                .ExposeAllNestedProperties(false)
                .AllowAnonymous();

            builder.BrowseSelectableEndpoint
                .HasEndpoint()
                .ExposeAllNestedProperties(false)
                .AllowAnonymous();

            builder.CreateEndpoint
                .HasEndpoint()
                .ExposeAllNestedProperties(false)
                .AllowAnonymous();

            builder.UpdateEndpoint
                .HasEndpoint()
                .ExposeAllNestedProperties(false)
                .AllowAnonymous();

            builder.DeleteEndpoint
                .HasEndpoint()
                .ExposeAllNestedProperties(false)
                .AllowAnonymous();
        }
    }
}