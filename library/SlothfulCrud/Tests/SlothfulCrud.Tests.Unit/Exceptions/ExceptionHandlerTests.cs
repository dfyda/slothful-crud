using System.Security.Authentication;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Exceptions.Handlers;

namespace SlothfulCrud.Tests.Unit.Exceptions
{
    public class ExceptionHandlerTests
    {
        private const int BadRequestStatusCode = 400;
        private const int UnauthorizedStatusCode = 401;
        private const int ForbiddenStatusCode = 403;
        private const int NotFoundStatusCode = 404;
        private const int InternalServerErrorStatusCode = 500;
        private const string ValidationErrorCode = "validation_error";
        private const string ConfigurationErrorCode = "configuration_error";
        private const string UnauthorizedCode = "unauthorized";
        private const string ForbiddenCode = "forbidden";
        private const string NotFoundCode = "entity_not_found";
        private const string InternalServerErrorCode = "internal_server_error";
        private const string ValidationErrorsKey = "validationErrors";

        [Fact]
        public void HandleError_ShouldMapNotFoundException_ToProblemDetailsContract()
        {
            // Arrange
            var handler = CreateHandler();
            var exception = new NotFoundException(NotFoundCode, "Entity was not found.");

            // Act
            var result = handler.HandleError(exception);

            // Assert
            AssertProblem(result.Status, result.Code, NotFoundStatusCode, NotFoundCode);
            Assert.Equal("Entity not found", result.Title);
            Assert.Equal("Entity was not found.", result.Detail);
            Assert.NotEqual(Guid.Empty, result.ProblemId);
        }

        [Fact]
        public void HandleError_ShouldMapValidationException_WithGroupedValidationErrors()
        {
            // Arrange
            var handler = CreateHandler();
            var exception = new ValidationException(new[]
            {
                new ValidationFailure("Name", "Name is required"),
                new ValidationFailure("Name", "Name is invalid"),
                new ValidationFailure("Age", "Age must be positive")
            });

            // Act
            var result = handler.HandleError(exception);

            // Assert
            AssertProblem(result.Status, result.Code, BadRequestStatusCode, ValidationErrorCode);
            Assert.Equal("Validation error", result.Title);
            Assert.True(result.Extensions.ContainsKey(ValidationErrorsKey));
            var validationErrors = Assert.IsType<Dictionary<string, string[]>>(result.Extensions[ValidationErrorsKey]);
            Assert.Equal(2, validationErrors["Name"].Length);
            Assert.Single(validationErrors["Age"]);
        }

        [Fact]
        public void HandleError_ShouldMapConfigurationException_ToForbiddenContract()
        {
            // Arrange
            var handler = CreateHandler();
            var exception = new ConfigurationException("Configuration is invalid.");

            // Act
            var result = handler.HandleError(exception);

            // Assert
            AssertProblem(result.Status, result.Code, ForbiddenStatusCode, ConfigurationErrorCode);
            Assert.Equal("Configuration error", result.Title);
            Assert.Equal("Configuration is invalid.", result.Detail);
        }

        [Fact]
        public void HandleError_ShouldMapAuthenticationException_ToUnauthorizedContract()
        {
            // Arrange
            var handler = CreateHandler();
            var exception = new AuthenticationException("Auth failed.");

            // Act
            var result = handler.HandleError(exception);

            // Assert
            AssertProblem(result.Status, result.Code, UnauthorizedStatusCode, UnauthorizedCode);
            Assert.Equal("Unauthorized", result.Title);
        }

        [Fact]
        public void HandleError_ShouldMapUnauthorizedAccessException_ToForbiddenContract()
        {
            // Arrange
            var handler = CreateHandler();
            var exception = new UnauthorizedAccessException("Forbidden.");

            // Act
            var result = handler.HandleError(exception);

            // Assert
            AssertProblem(result.Status, result.Code, ForbiddenStatusCode, ForbiddenCode);
            Assert.Equal("Forbidden", result.Title);
            Assert.Equal("Access to this resource is forbidden.", result.Detail);
        }

        [Fact]
        public void HandleError_ShouldMapUnknownException_ToInternalServerErrorContract()
        {
            // Arrange
            var handler = CreateHandler();
            var exception = new InvalidOperationException("Unexpected.");

            // Act
            var result = handler.HandleError(exception);

            // Assert
            AssertProblem(result.Status, result.Code, InternalServerErrorStatusCode, InternalServerErrorCode);
            Assert.Equal("Internal server error", result.Title);
            Assert.NotEqual(Guid.Empty, result.ProblemId);
        }

        private static ExceptionHandler CreateHandler()
        {
            var logger = new Mock<ILogger<ExceptionHandler>>();
            return new ExceptionHandler(logger.Object);
        }

        private static void AssertProblem(int? status, string code, int expectedStatus, string expectedCode)
        {
            Assert.Equal(expectedStatus, status);
            Assert.Equal(expectedCode, code);
        }
    }
}
