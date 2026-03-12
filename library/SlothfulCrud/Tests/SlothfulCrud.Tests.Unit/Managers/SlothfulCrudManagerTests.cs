using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Extensions;
using SlothfulCrud.Managers;
using SlothfulCrud.Providers;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Tests.Api.EF;

namespace SlothfulCrud.Tests.Unit.Managers
{
    public class SlothfulCrudManagerTests
    {
        private const string CreateSlothEndpoint = "CreateSloth";
        private const string UpdateSlothEndpoint = "UpdateSloth";
        private const string DeleteSlothEndpoint = "DeleteSloth";
        private const string BrowseSelectableSlothsEndpoint = "BrowseSelectableSloths";
        private const string GetWildKoalaDetailsEndpoint = "GetWildKoalaDetails";
        private const string BrowseWildKoalasEndpoint = "BrowseWildKoalas";
        private const string BrowseSelectableWildKoalasEndpoint = "BrowseSelectableWildKoalas";
        private const string CreateWildKoalaEndpoint = "CreateWildKoala";
        private const string UpdateWildKoalaEndpoint = "UpdateWildKoala";
        private const string DeleteWildKoalaEndpoint = "DeleteWildKoala";
        private const string GetSlothDetailsEndpoint = "GetSlothDetails";
        private const string BrowseSlothsEndpoint = "BrowseSloths";

        [Fact]
        public void Register_ShouldMapEndpoints_AndRespectDisabledEndpoints_ForEntitiesFromAssembly()
        {
            // Arrange
            var app = CreateApplication();
            var manager = new SlothfulCrudManager(new ApiSegmentProvider(), new BaseCreateConstructorBehavior());

            // Act
            manager.Register(app, typeof(SlothfulDbContext), typeof(Sloth).Assembly);

            // Assert
            var endpointNames = GetEndpointNames(app);

            Assert.Contains(CreateSlothEndpoint, endpointNames);
            Assert.Contains(UpdateSlothEndpoint, endpointNames);
            Assert.Contains(DeleteSlothEndpoint, endpointNames);
            Assert.Contains(BrowseSelectableSlothsEndpoint, endpointNames);

            Assert.Contains(GetWildKoalaDetailsEndpoint, endpointNames);
            Assert.Contains(BrowseWildKoalasEndpoint, endpointNames);
            Assert.Contains(BrowseSelectableWildKoalasEndpoint, endpointNames);
            Assert.Contains(CreateWildKoalaEndpoint, endpointNames);
            Assert.Contains(UpdateWildKoalaEndpoint, endpointNames);
            Assert.Contains(DeleteWildKoalaEndpoint, endpointNames);
            Assert.DoesNotContain(GetSlothDetailsEndpoint, endpointNames);
            Assert.DoesNotContain(BrowseSlothsEndpoint, endpointNames);
        }

        private static WebApplication CreateApplication()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Services.AddLogging();
            builder.Services.AddDbContext<SlothfulDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            builder.Services.AddValidatorsFromAssemblyContaining<SlothfulCrud.Tests.Api.Validators.SlothValidator>();
            builder.Services.AddSlothfulCrud<SlothfulDbContext>();
            return builder.Build();
        }

        private static List<string> GetEndpointNames(WebApplication app)
        {
            return ((IEndpointRouteBuilder)app).DataSources
                .SelectMany(source => source.Endpoints)
                .Select(endpoint => endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();
        }
    }
}
