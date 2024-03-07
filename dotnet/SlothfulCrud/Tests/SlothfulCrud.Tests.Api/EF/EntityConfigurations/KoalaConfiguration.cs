using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlothfulCrud.Tests.Api.Domain;

namespace SlothfulCrud.Tests.Api.EF.EntityConfigurations
{
    public class KoalaConfiguration : IEntityTypeConfiguration<Koala>
    {
        public void Configure(EntityTypeBuilder<Koala> builder)
        {
            // builder.HasNoKey();
        }
    }
}