using System.Net;
using System.Security.Authentication;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SlothfulCrud.Extensions;

namespace SlothfulCrud.Exceptions.Handlers
{
    public class ExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(ILogger<ExceptionHandler> logger)
        {
            _logger = logger;
        }
        
        public SlothProblemDetails HandleError(Exception exception)
        {
            var exceptionType = exception.GetType();
            var problemId = Guid.NewGuid();

            var response = exception switch
            {
                NotFoundException notFoundException when exceptionType == typeof(NotFoundException) => HandleNotFoundException(notFoundException,
                    problemId),
                ValidationException validationException when exceptionType == typeof(ValidationException) =>
                    HandleValidationException(validationException, problemId),
                ConfigurationException configurationException when exceptionType == typeof(ConfigurationException) =>
                    HandleConfigurationException(configurationException, problemId),
                AuthenticationException authenticationException when exceptionType == typeof(AuthenticationException) =>
                    HandleAuthenticationException(authenticationException, problemId),
                UnauthorizedAccessException unauthorizedAccessException when exceptionType == typeof(UnauthorizedAccessException) =>
                    HandleUnauthorizedAccessException(unauthorizedAccessException, problemId),
                _ => HandleException(exception, problemId)
            };

            HandleResponse(response, exception);

            return response;
        }

        private void HandleResponse(SlothProblemDetails response, Exception exception)
        {
            _logger.LogCritical(exception, "An error occurred for ProblemId: '{ProblemId}', message: '{Message}'", response.ProblemId, exception.Message);
        }

        private SlothProblemDetails HandleNotFoundException(NotFoundException exception, Guid problemId)
        {
            var httpResponseCode = HttpStatusCode.NotFound;

            var result = PrepareHandledResponse(
                problemId,
                httpResponseCode,
                exception.Code,
                exception.Message);

            return result;
        }

        private SlothProblemDetails PrepareHandledResponse(
            Guid problemId,
            HttpStatusCode httpResponseCode,
            string code,
            string message)
        {
            return new SlothProblemDetails()
            {
                ProblemId = problemId,
                Title = code.SnakeCaseToHumanReadable(),
                Detail = message,
                Code = code,
                Status = (int)httpResponseCode
            };
        }

        private SlothProblemDetails HandleValidationException(ValidationException validationException, Guid problemId)
        {
            var httpResponseCode = HttpStatusCode.BadRequest;

            var result = PrepareHandledResponse(
                problemId,
                httpResponseCode,
                "validation_error",
                "One or more validation errors occurred. Please refer to the validationErrors for detailed information.");
            
            result.Extensions["validationErrors"] = validationException.Errors
                .GroupBy(validationFailure => validationFailure.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return result;
        }
        
        private SlothProblemDetails HandleConfigurationException(ConfigurationException exception, Guid problemId)
        {
            var httpResponseCode = HttpStatusCode.Forbidden;

            var result = PrepareHandledResponse(
                problemId,
                httpResponseCode,
                "configuration_error",
                exception.Message);

            return result;
        }

        private SlothProblemDetails HandleForbiddenResponse(Exception exception, Guid problemId)
        {
            var result = PrepareHandledResponse(
                problemId,
                HttpStatusCode.Forbidden,
                "forbidden",
                "Access to this resource is forbidden.");

            return result;
        }

        private SlothProblemDetails HandleAuthenticationException(AuthenticationException exception, Guid problemId)
        {
            var httpResponseCode = HttpStatusCode.Unauthorized;

            var result = PrepareHandledResponse(
                problemId,
                httpResponseCode,
                "unauthorized",
                "Authentication failed due to invalid credentials. Please verify your credentials and try again.");

            return result;
        }

        private SlothProblemDetails HandleUnauthorizedAccessException(UnauthorizedAccessException exception, Guid problemId)
        {
            return HandleForbiddenResponse(exception, problemId);
        }

        private SlothProblemDetails HandleException(Exception exception, Guid problemId)
        {
            var httpResponseCode = HttpStatusCode.InternalServerError;

            var result = PrepareHandledResponse(
                problemId,
                httpResponseCode,
                "internal_server_error",
                "The server encountered an internal error and was unable to complete your request. Please try again later or contact support if the issue persists.");

            return result;
        }
    }
}