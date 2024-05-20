namespace SlothfulCrud.Exceptions.Handlers
{
    public interface IExceptionHandler
    {
        SlothProblemDetails HandleError(Exception exception);
    }
}