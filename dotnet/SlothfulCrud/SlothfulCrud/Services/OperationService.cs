using SlothfulCrud.Domain;

namespace SlothfulCrud.Services
{
    public class OperationService<T> : IOperationService<T> where T : class, ISlothfulEntity, new()
    {
        public T Get()
        {
            return new T();
        }
    }
}