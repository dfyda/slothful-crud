using SlothfulCrud.Domain;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Providers
{
    public interface IEntityPropertyKeyValueProvider<TEntity> where TEntity : class, ISlothfulEntity
    {
        object GetNextValue(EntityConfiguration configuration);
    }
}