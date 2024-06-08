using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SlothfulCrud.Exceptions.Handlers;

namespace SlothfulCrud.Exceptions.Middlewares
{
    internal class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IExceptionHandler _exceptionHandler;

        public ExceptionMiddleware(
            RequestDelegate next,
            IExceptionHandler exceptionHandler)
        {
            _next = next;
            _exceptionHandler = exceptionHandler;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await ProcessExceptionAsync(context, exception);
            }
        }

        private Task ProcessExceptionAsync(HttpContext context, Exception exception)
        {
            var response = _exceptionHandler.HandleError(exception);

            return CompleteAsync(context, response);
        }

        private Task CompleteAsync(HttpContext context, SlothProblemDetails problemDetails)
        {
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

            return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}