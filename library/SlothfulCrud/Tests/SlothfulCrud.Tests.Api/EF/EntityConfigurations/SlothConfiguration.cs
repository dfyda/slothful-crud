using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlothfulCrud.Tests.Api.Domain;

namespace SlothfulCrud.Tests.Api.EF.EntityConfigurations
{
    public class SlothConfiguration : IEntityTypeConfiguration<Sloth>
    {
        public void Configure(EntityTypeBuilder<Sloth> builder)
        {
            // builder.HasNoKey();
        }
    }
}