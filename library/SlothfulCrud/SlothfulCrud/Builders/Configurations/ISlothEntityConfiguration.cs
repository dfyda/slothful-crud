using SlothfulCrud.Domain;

namespace SlothfulCrud.Builders.Configurations
{
    public interface ISlothEntityConfiguration<T> where T : class, ISlothfulEntity
    {
        void Configure(SlothEntityBuilder<T> builder);
    }
}