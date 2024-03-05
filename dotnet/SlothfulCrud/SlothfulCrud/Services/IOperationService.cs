using SlothfulCrud.Domain;

namespace SlothfulCrud.Services
{
    public interface IOperationService<T> where T : ISlothfulEntity
    {
        T Get();
    }
}

