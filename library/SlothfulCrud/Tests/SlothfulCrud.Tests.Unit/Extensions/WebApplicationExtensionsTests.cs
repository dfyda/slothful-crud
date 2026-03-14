using System.Net;
using System.Security.Authentication;
using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Exceptions.Handlers;
using SlothfulCrud.Extensions;
using SlothfulCrud.Managers;

namespace SlothfulCrud.Tests.Unit.Extensions
{
    public class WebApplicationExtensionsTests
    {
        private const string ThrowRoute = "/throw";
        private const string SecureRoute = "/secure";
        private const string SecuredPayload = "secured";
        private const string ProblemJsonMediaType = "application/problem+json";
        private const string ValidationErrorCode = "validation_error";
        private const string ConfigurationErrorCode = "configuration_error";
        private const string EntityNotFoundCode = "entity_not_found";
        private const string UnauthorizedCode = "unauthorized";
        private const string ForbiddenCode = "forbidden";
        private const string InternalServerErrorCode = "internal_server_error";
        private const string ProblemCodeProperty = "Code";
        private const string ProblemTitleProperty = "title";
        private const string ProblemStatusProperty = "status";
        private const string ProblemIdProperty = "ProblemId";
        private const string ValidationErrorsProperty = "validationErrors";
        private const string ValidationErrorField = "Name";
        private const string WritersOnlyPolicy = "WritersOnly";
        private const string WriterRole = "writer";
        private const string ReaderRole = "reader";
        private const string TestUserName = "test-user";

        [Fact]
        public void UseSlothfulCrud_ShouldCallCrudManagerRegister()
        {
            // Arrange
            var capturingManager = new CapturingCrudManager();
            var app = CreateTestApplication(capturingManager);

            // Act
            app.UseSlothfulCrud<FakeDbContext>();

            // Assert
            Assert.True(capturingManager.WasCalled);
            Assert.Equal(typeof(FakeDbContext), capturingManager.CapturedDbContextType);
            Assert.NotNull(capturingManager.CapturedAssembly);
        }

        [Fact]
        public void UseSlothfulCrudWithMarker_ShouldPassMarkerAssemblyToManager()
        {
            // Arrange
            var capturingManager = new CapturingCrudManager();
            var app = CreateTestApplication(capturingManager);

            // Act
            app.UseSlothfulCrud<FakeDbContext, FakeDbContext>();

            // Assert
            Assert.True(capturingManager.WasCalled);
            Assert.Equal(typeof(FakeDbContext), capturingManager.CapturedDbContextType);
            Assert.Equal(typeof(FakeDbContext).Assembly, capturingManager.CapturedAssembly);
        }

        [Fact]
        public void UseSlothfulCrudWithMarker_ShouldRetrieveOptionsFromDI()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Services.AddLogging();
            builder.Services.AddScoped<ISlothfulCrudManager, PassthroughCrudManager>();
            builder.Services.AddTransient<IExceptionHandler, ExceptionHandler>();
            builder.Services.AddSingleton(new SlothfulCrud.Types.Configurations.SlothfulOptions
            {
                UseSlothfulProblemHandling = true
            });
            var app = builder.Build();

            // Act + Assert (no exception = options resolved from DI)
            app.UseSlothfulCrud<FakeDbContext, FakeDbContext>();
        }

        [Fact]
        public void UseSlothfulCrudLegacy_ShouldHaveObsoleteAttribute()
        {
            var method = typeof(WebApplicationExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == "UseSlothfulCrud" && m.GetGenericArguments().Length == 1
                             && m.GetParameters().Length == 2);

            Assert.True(method.IsDefined(typeof(ObsoleteAttribute), false));
        }

        [Fact]
        public async Task UseSlothfulCrud_ShouldReturnProblemDetails_WhenProblemHandlingIsEnabled()
        {
            // Arrange
            await using var app = CreateTestApplication();
            app.UseSlothfulCrud<FakeDbContext>(options => options.UseSlothfulProblemHandling = true);
            app.MapGet(ThrowRoute, (HttpContext _) => throw new InvalidOperationException("boom"));
            await app.StartAsync();
            var client = app.GetTestClient();

            // Act
            var response = await client.GetAsync(ThrowRoute);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            AssertProblemDetailsResponse(response, responseBody, HttpStatusCode.InternalServerError, InternalServerErrorCode);
        }

        [Fact]
        public async Task UseSlothfulCrud_ShouldMapConfigurationException_WhenProblemHandlingIsEnabled()
        {
            // Arrange
            await using var app = CreateTestApplication();
            app.UseSlothfulCrud<FakeDbContext>(options => options.UseSlothfulProblemHandling = true);
            app.MapGet(ThrowRoute, (HttpContext _) => throw new ConfigurationException("cfg_error"));
            await app.StartAsync();
            var client = app.GetTestClient();

            // Act
            var response = await client.GetAsync(ThrowRoute);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            AssertProblemDetailsResponse(response, responseBody, HttpStatusCode.Forbidden, ConfigurationErrorCode);
        }

        [Fact]
        public async Task UseSlothfulCrud_ShouldMapValidationException_WhenProblemHandlingIsEnabled()
        {
            // Arrange
            var failures = new[] { new ValidationFailure("Name", "Name is invalid") };
            await using var app = CreateTestApplication();
            app.UseSlothfulCrud<FakeDbContext>(options => options.UseSlothfulProblemHandling = true);
            app.MapGet(ThrowRoute, (HttpContext _) => throw new ValidationException(failures));
            await app.StartAsync();
            var client = app.GetTestClient();

            // Act
            var response = await client.GetAsync(ThrowRoute);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            AssertProblemDetailsResponse(response, responseBody, HttpStatusCode.BadRequest, ValidationErrorCode);
            Assert.Contains(ValidationErrorsProperty, responseBody);
        }

        [Fact]
        public async Task UseSlothfulCrud_ShouldReturnCompleteProblemDetailsContract_WhenValidationExceptionIsHandled()
        {
            // Arrange
            var failures = new[]
            {
                new ValidationFailure("Name", "Name is invalid"),
                new ValidationFailure("Name", "Name is required")
            };
            await using var app = CreateTestApplication();
            app.UseSlothfulCrud<FakeDbContext>(options => options.UseSlothfulProblemHandling = true);
            app.MapGet(ThrowRoute, (HttpContext _) => throw new ValidationException(failures));
            await app.StartAsync();
            var client = app.GetTestClient();

            // Act
            var response = await client.GetAsync(ThrowRoute);
            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(ProblemJsonMediaType, response.Content.Headers.ContentType?.MediaType);
            Assert.Equal(ValidationErrorCode, root.GetProperty(ProblemCodeProperty).GetString());
            Assert.Equal("Validation error", root.GetProperty(ProblemTitleProperty).GetString());
            Assert.Equal(StatusCodes.Status400BadRequest, root.GetProperty(ProblemStatusProperty).GetInt32());
            Assert.True(root.TryGetProperty(ProblemIdProperty, out var problemIdProperty));
            Assert.NotEqual(Guid.Empty, problemIdProperty.GetGuid());
            Assert.True(root.TryGetProperty(ValidationErrorsProperty, out var validationErrors));
            Assert.True(validationErrors.TryGetProperty(ValidationErrorField, out var nameErrors));
            Assert.Equal(2, nameErrors.GetArrayLength());
        }

        [Fact]
        public async Task UseSlothfulCrud_ShouldMapNotFoundException_WhenProblemHandlingIsEnabled()
        {
            // Arrange
            await using var app = CreateTestApplication();
            app.UseSlothfulCrud<FakeDbContext>(options => options.UseSlothfulProblemHandling = true);
            app.MapGet(ThrowRoute, (HttpContext _) => throw new NotFoundException(EntityNotFoundCode, "Entity was not found."));
            await app.StartAsync();
            var client = app.GetTestClient();

            // Act
            var response = await client.GetAsync(ThrowRoute);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            AssertProblemDetailsResponse(response, responseBody, HttpStatusCode.NotFound, EntityNotFoundCode);
        }

        [Fact]
        public async Task UseSlothfulCrud_ShouldMapAuthenticationException_WhenProblemHandlingIsEnabled()
        {
            // Arrange
            await using var app = CreateTestApplication();
            app.UseSlothfulCrud<FakeDbContext>(options => options.UseSlothfulProblemHandling = true);
            app.MapGet(ThrowRoute, (HttpContext _) => throw new AuthenticationException("Auth failed"));
            await app.StartAsync();
            var client = app.GetTestClient();

            // Act
            var response = await client.GetAsync(ThrowRoute);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            AssertProblemDetailsResponse(response, responseBody, HttpStatusCode.Unauthorized, UnauthorizedCode);
        }

        [Fact]
        public async Task UseSlothfulCrud_ShouldMapUnauthorizedAccessException_WhenProblemHandlingIsEnabled()
        {
            // Arrange
            await using var app = CreateTestApplication();
            app.UseSlothfulCrud<FakeDbContext>(options => options.UseSlothfulProblemHandling = true);
            app.MapGet(ThrowRoute, (HttpContext _) => throw new UnauthorizedAccessException("Forbidden"));
            await app.StartAsync();
            var client = app.GetTestClient();

            // Act
            var response = await client.GetAsync(ThrowRoute);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Assert
            AssertProblemDetailsResponse(response, responseBody, HttpStatusCode.Forbidden, ForbiddenCode);
        }

        [Fact]
        public async Task UseSlothfulCrud_ShouldNotHandleException_WhenProblemHandlingIsDisabled()
        {
            // Arrange
            await using var app = CreateTestApplication();
            app.UseSlothfulCrud<FakeDbContext>(options => options.UseSlothfulProblemHandling = false);
            app.MapGet(ThrowRoute, (HttpContext _) => throw new InvalidOperationException("boom"));
            await app.StartAsync();
            var client = app.GetTestClient();

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() => client.GetAsync(ThrowRoute));
        }

        [Fact]
        public async Task AuthorizationPipeline_ShouldReturnUnauthorized_WhenRequestIsUnauthenticated()
        {
            // Arrange
            await using var app = await CreateAuthorizedApplication();
            var client = app.GetTestClient();

            // Act
            var response = await client.GetAsync(SecureRoute);

            // Assert
            AssertAuthorizationOutcome(response, HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task AuthorizationPipeline_ShouldReturnForbidden_WhenUserDoesNotMeetPolicy()
        {
            // Arrange
            await using var app = await CreateAuthorizedApplication();
            var client = app.GetTestClient();
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeaderName, ReaderRole);

            // Act
            var response = await client.GetAsync(SecureRoute);

            // Assert
            AssertAuthorizationOutcome(response, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task AuthorizationPipeline_ShouldReturnOk_WhenUserMeetsPolicy()
        {
            // Arrange
            await using var app = await CreateAuthorizedApplication();
            var client = app.GetTestClient();
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeaderName, WriterRole);

            // Act
            var response = await client.GetAsync(SecureRoute);
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            AssertAuthorizationOutcome(response, HttpStatusCode.OK);
            Assert.Equal(SecuredPayload, body);
        }

        [Fact]
        public async Task AuthorizationPipeline_ShouldWork_WhenUseSlothfulCrudRunsBeforeAuthenticationMiddleware()
        {
            // Arrange
            await using var app = await CreateAuthorizedApplication(useSlothfulCrudBeforeAuth: true);
            var client = app.GetTestClient();
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeaderName, WriterRole);

            // Act
            var response = await client.GetAsync(SecureRoute);

            // Assert
            AssertAuthorizationOutcome(response, HttpStatusCode.OK);
        }

        private static WebApplication CreateTestApplication(ISlothfulCrudManager manager = null)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Services.AddLogging();
            if (manager is null)
            {
                builder.Services.AddScoped<ISlothfulCrudManager, PassthroughCrudManager>();
            }
            else
            {
                builder.Services.AddSingleton<ISlothfulCrudManager>(manager);
            }

            builder.Services.AddTransient<IExceptionHandler, ExceptionHandler>();

            return builder.Build();
        }

        private static async Task<WebApplication> CreateAuthorizedApplication(bool useSlothfulCrudBeforeAuth = false)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();
            builder.Services.AddLogging();
            builder.Services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(WritersOnlyPolicy, policy =>
                    policy.RequireClaim(ClaimTypes.Role, WriterRole));
            });
            builder.Services.AddScoped<ISlothfulCrudManager, PassthroughCrudManager>();
            builder.Services.AddTransient<IExceptionHandler, ExceptionHandler>();

            var app = builder.Build();
            if (useSlothfulCrudBeforeAuth)
            {
                app.UseSlothfulCrud<FakeDbContext>(options => options.UseSlothfulProblemHandling = false);
            }

            app.UseAuthentication();
            app.UseAuthorization();

            if (!useSlothfulCrudBeforeAuth)
            {
                app.UseSlothfulCrud<FakeDbContext>(options => options.UseSlothfulProblemHandling = false);
            }

            app.MapGet(SecureRoute, () => SecuredPayload).RequireAuthorization(WritersOnlyPolicy);

            await app.StartAsync();
            return app;
        }

        private static void AssertProblemDetailsResponse(
            HttpResponseMessage response,
            string responseBody,
            HttpStatusCode expectedStatusCode,
            string expectedCode)
        {
            Assert.Equal(expectedStatusCode, response.StatusCode);
            Assert.Equal(ProblemJsonMediaType, response.Content.Headers.ContentType?.MediaType);
            Assert.Contains($"\"{ProblemCodeProperty}\":\"{expectedCode}\"", responseBody);
        }

        private static void AssertAuthorizationOutcome(HttpResponseMessage response, HttpStatusCode expectedStatusCode)
        {
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        private class PassthroughCrudManager : ISlothfulCrudManager
        {
            public WebApplication Register(WebApplication webApplication, Type dbContextType, System.Reflection.Assembly executingAssembly)
            {
                return webApplication;
            }
        }

        private class CapturingCrudManager : ISlothfulCrudManager
        {
            public bool WasCalled { get; private set; }
            public Type CapturedDbContextType { get; private set; }
            public Assembly CapturedAssembly { get; private set; }

            public WebApplication Register(WebApplication webApplication, Type dbContextType, Assembly executingAssembly)
            {
                WasCalled = true;
                CapturedDbContextType = dbContextType;
                CapturedAssembly = executingAssembly;
                return webApplication;
            }
        }

        private class FakeDbContext : DbContext
        {
            public FakeDbContext(DbContextOptions<FakeDbContext> options) : base(options)
            {
            }
        }

        private class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public const string SchemeName = "TestScheme";
            public const string RoleHeaderName = "X-Test-Role";

            public TestAuthHandler(
                IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger,
                UrlEncoder encoder)
                : base(options, logger, encoder)
            {
            }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                if (!Request.Headers.TryGetValue(RoleHeaderName, out var role) || string.IsNullOrWhiteSpace(role))
                {
                    return Task.FromResult(AuthenticateResult.NoResult());
                }

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, TestUserName),
                    new Claim(ClaimTypes.Role, role.ToString())
                };
                var identity = new ClaimsIdentity(claims, SchemeName);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, SchemeName);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }
    }
}
