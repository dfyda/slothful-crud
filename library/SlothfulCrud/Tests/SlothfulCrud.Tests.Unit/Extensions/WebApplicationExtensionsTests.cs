using System.Net;
using System.Security.Authentication;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Exceptions.Handlers;
using SlothfulCrud.Extensions;
using SlothfulCrud.Managers;

namespace SlothfulCrud.Tests.Unit.Extensions
{
    public class WebApplicationExtensionsTests
    {
        [Fact]
        public async Task UseSlothfulCrud_ShouldReturnProblemDetails_WhenProblemHandlingIsEnabled()
        {
            // Arrange
            await using var app = CreateTestApplication();
            app.UseSlothfulCrud<FakeDbContext>(options => options.UseSlothfulProblemHandling = true);
            app.MapGet("/throw", (HttpContext _) => throw new InvalidOperationException("boom"));
            await app.StartAsync();
            var client = app.GetTestClient();

            // Act
            var response = await client.GetAsync("/throw");
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Contains("\"Code\":\"internal_server_error\"", responseBody);
        }

        [Fact]
        public async Task UseSlothfulCrud_ShouldMapConfigurationException_WhenProblemHandlingIsEnabled()
        {
            // Arrange
            await using var app = CreateTestApplication();
            app.UseSlothfulCrud<FakeDbContext>(options => options.UseSlothfulProblemHandling = true);
            app.MapGet("/throw", (HttpContext _) => throw new ConfigurationException("cfg_error"));
            await app.StartAsync();
            var client = app.GetTestClient();

            // Act
            var response = await client.GetAsync("/throw");
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Contains("\"Code\":\"configuration_error\"", responseBody);
        }

        [Fact]
        public async Task UseSlothfulCrud_ShouldMapValidationException_WhenProblemHandlingIsEnabled()
        {
            // Arrange
            var failures = new[] { new ValidationFailure("Name", "Name is invalid") };
            await using var app = CreateTestApplication();
            app.UseSlothfulCrud<FakeDbContext>(options => options.UseSlothfulProblemHandling = true);
            app.MapGet("/throw", (HttpContext _) => throw new ValidationException(failures));
            await app.StartAsync();
            var client = app.GetTestClient();

            // Act
            var response = await client.GetAsync("/throw");
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Contains("\"Code\":\"validation_error\"", responseBody);
            Assert.Contains("validationErrors", responseBody);
        }

        [Fact]
        public async Task UseSlothfulCrud_ShouldMapNotFoundException_WhenProblemHandlingIsEnabled()
        {
            // Arrange
            await using var app = CreateTestApplication();
            app.UseSlothfulCrud<FakeDbContext>(options => options.UseSlothfulProblemHandling = true);
            app.MapGet("/throw", (HttpContext _) => throw new NotFoundException("entity_not_found", "Entity was not found."));
            await app.StartAsync();
            var client = app.GetTestClient();

            // Act
            var response = await client.GetAsync("/throw");
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Contains("\"Code\":\"entity_not_found\"", responseBody);
        }

        [Fact]
        public async Task UseSlothfulCrud_ShouldMapAuthenticationException_WhenProblemHandlingIsEnabled()
        {
            // Arrange
            await using var app = CreateTestApplication();
            app.UseSlothfulCrud<FakeDbContext>(options => options.UseSlothfulProblemHandling = true);
            app.MapGet("/throw", (HttpContext _) => throw new AuthenticationException("Auth failed"));
            await app.StartAsync();
            var client = app.GetTestClient();

            // Act
            var response = await client.GetAsync("/throw");
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Contains("\"Code\":\"unauthorized\"", responseBody);
        }

        [Fact]
        public async Task UseSlothfulCrud_ShouldMapUnauthorizedAccessException_WhenProblemHandlingIsEnabled()
        {
            // Arrange
            await using var app = CreateTestApplication();
            app.UseSlothfulCrud<FakeDbContext>(options => options.UseSlothfulProblemHandling = true);
            app.MapGet("/throw", (HttpContext _) => throw new UnauthorizedAccessException("Forbidden"));
            await app.StartAsync();
            var client = app.GetTestClient();

            // Act
            var response = await client.GetAsync("/throw");
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            Assert.Contains("\"Code\":\"forbidden\"", responseBody);
        }

        [Fact]
        public async Task UseSlothfulCrud_ShouldNotHandleException_WhenProblemHandlingIsDisabled()
        {
            // Arrange
            await using var app = CreateTestApplication();
            app.UseSlothfulCrud<FakeDbContext>(options => options.UseSlothfulProblemHandling = false);
            app.MapGet("/throw", (HttpContext _) => throw new InvalidOperationException("boom"));
            await app.StartAsync();
            var client = app.GetTestClient();

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() => client.GetAsync("/throw"));
        }

        private static WebApplication CreateTestApplication()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Services.AddLogging();
            builder.Services.AddScoped<ISlothfulCrudManager, PassthroughCrudManager>();
            builder.Services.AddTransient<IExceptionHandler, ExceptionHandler>();

            return builder.Build();
        }

        private class PassthroughCrudManager : ISlothfulCrudManager
        {
            public WebApplication Register(WebApplication webApplication, Type dbContextType, System.Reflection.Assembly executingAssembly)
            {
                return webApplication;
            }
        }

        private class FakeDbContext : DbContext
        {
            public FakeDbContext(DbContextOptions<FakeDbContext> options) : base(options)
            {
            }
        }
    }
}
