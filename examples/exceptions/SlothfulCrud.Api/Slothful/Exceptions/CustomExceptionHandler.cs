using SlothfulCrud.Exceptions;
using SlothfulCrud.Exceptions.Handlers;

namespace SlothfulCrud.Api.Slothful.Exceptions
{
    public class CustomExceptionHandler : IExceptionHandler
    {
        public SlothProblemDetails HandleError(Exception exception)
        {
            return new SlothProblemDetails()
            {
                ProblemId = Guid.NewGuid(),
                Title = "An error occurred",
                Detail = "An error occurred while processing the request",
                Code = "CustomError",
                Status = 418
            };
        }
    }
}