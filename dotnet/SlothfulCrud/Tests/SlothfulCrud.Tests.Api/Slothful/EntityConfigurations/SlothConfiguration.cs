﻿using SlothfulCrud.Builders.Configurations;
using SlothfulCrud.Tests.Api.Domain;

namespace SlothfulCrud.Tests.Api.Slothful.EntityConfigurations
{
    public class SlothConfiguration : ISlothEntityConfiguration<Sloth>
    {
        public void Configure(SlothEntityBuilder<Sloth> builder)
        {
            builder
                .SetSortProperty(x => x.DisplayName)
                .SetUpdateMethodName("Update");
            
            builder.GetEndpoint
                .ExposeAllNestedProperties()
                .AllowAnonymous();
            
            builder.BrowseEndpoint
                .ExposeAllNestedProperties()
                .AllowAnonymous();
        }
    }
}