using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SlothfulCrud.Builders.Configurations;
using SlothfulCrud.Builders.Endpoints;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Builders.Endpoints.Parameters;
using SlothfulCrud.Domain;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Tests.Unit.Builders.Endpoints
{
    public class AuthorizationMetadataTests
    {
        [Fact]
        public void GetEndpoint_Map_ShouldAttachAuthorizeMetadata_WhenRequireAuthorizationIsConfigured()
        {
            AssertEndpointHasAuthorizeMetadata<GetEntity>(
                builder => builder.GetEndpoint.RequireAuthorization("Policy.Get"),
                routeBuilder => routeBuilder.GetEndpoint.Map(),
                "GetGetEntityDetails",
                "Policy.Get");
        }

        [Fact]
        public void BrowseEndpoint_Map_ShouldAttachAuthorizeMetadata_WhenRequireAuthorizationIsConfigured()
        {
            AssertEndpointHasAuthorizeMetadata<BrowseEntity>(
                builder => builder.BrowseEndpoint.RequireAuthorization("Policy.Browse"),
                routeBuilder => routeBuilder.BrowseEndpoint.Map(),
                "BrowseBrowseEntitys",
                "Policy.Browse");
        }

        [Fact]
        public void BrowseSelectableEndpoint_Map_ShouldAttachAuthorizeMetadata_WhenRequireAuthorizationIsConfigured()
        {
            AssertEndpointHasAuthorizeMetadata<BrowseSelectableEntity>(
                builder => builder.BrowseSelectableEndpoint.RequireAuthorization("Policy.BrowseSelectable"),
                routeBuilder => routeBuilder.BrowseSelectableEndpoint.Map(),
                "BrowseSelectableBrowseSelectableEntitys",
                "Policy.BrowseSelectable");
        }

        [Fact]
        public void CreateEndpoint_Map_ShouldAttachAuthorizeMetadata_WhenRequireAuthorizationIsConfigured()
        {
            AssertEndpointHasAuthorizeMetadata<CreateEntity>(
                builder => builder.CreateEndpoint.RequireAuthorization("Policy.Create"),
                routeBuilder => routeBuilder.CreateEndpoint.Map(),
                "CreateCreateEntity",
                "Policy.Create");
        }

        [Fact]
        public void UpdateEndpoint_Map_ShouldAttachAuthorizeMetadata_WhenRequireAuthorizationIsConfigured()
        {
            AssertEndpointHasAuthorizeMetadata<UpdateEntity>(
                builder => builder.UpdateEndpoint.RequireAuthorization("Policy.Update"),
                routeBuilder => routeBuilder.UpdateEndpoint.Map(),
                "UpdateUpdateEntity",
                "Policy.Update");
        }

        [Fact]
        public void DeleteEndpoint_Map_ShouldAttachAuthorizeMetadata_WhenRequireAuthorizationIsConfigured()
        {
            AssertEndpointHasAuthorizeMetadata<DeleteEntity>(
                builder => builder.DeleteEndpoint.RequireAuthorization("Policy.Delete"),
                routeBuilder => routeBuilder.DeleteEndpoint.Map(),
                "DeleteDeleteEntity",
                "Policy.Delete");
        }

        [Fact]
        public void GetEndpoint_Map_ShouldNotMapEndpoint_WhenHasEndpointIsFalse()
        {
            AssertEndpointIsNotMapped<GetEntity>(
                builder => builder.GetEndpoint.HasEndpoint(false),
                routeBuilder => routeBuilder.GetEndpoint.Map());
        }

        [Fact]
        public void BrowseEndpoint_Map_ShouldNotMapEndpoint_WhenHasEndpointIsFalse()
        {
            AssertEndpointIsNotMapped<BrowseEntity>(
                builder => builder.BrowseEndpoint.HasEndpoint(false),
                routeBuilder => routeBuilder.BrowseEndpoint.Map());
        }

        [Fact]
        public void BrowseSelectableEndpoint_Map_ShouldNotMapEndpoint_WhenHasEndpointIsFalse()
        {
            AssertEndpointIsNotMapped<BrowseSelectableEntity>(
                builder => builder.BrowseSelectableEndpoint.HasEndpoint(false),
                routeBuilder => routeBuilder.BrowseSelectableEndpoint.Map());
        }

        [Fact]
        public void CreateEndpoint_Map_ShouldNotMapEndpoint_WhenHasEndpointIsFalse()
        {
            AssertEndpointIsNotMapped<CreateEntity>(
                builder => builder.CreateEndpoint.HasEndpoint(false),
                routeBuilder => routeBuilder.CreateEndpoint.Map());
        }

        [Fact]
        public void UpdateEndpoint_Map_ShouldNotMapEndpoint_WhenHasEndpointIsFalse()
        {
            AssertEndpointIsNotMapped<UpdateEntity>(
                builder => builder.UpdateEndpoint.HasEndpoint(false),
                routeBuilder => routeBuilder.UpdateEndpoint.Map());
        }

        [Fact]
        public void DeleteEndpoint_Map_ShouldNotMapEndpoint_WhenHasEndpointIsFalse()
        {
            AssertEndpointIsNotMapped<DeleteEntity>(
                builder => builder.DeleteEndpoint.HasEndpoint(false),
                routeBuilder => routeBuilder.DeleteEndpoint.Map());
        }

        [Fact]
        public void UpdateEndpoint_Map_ShouldNotAttachAuthorizeMetadata_WhenAllowAnonymousOverridesRequireAuthorization()
        {
            // Arrange
            var app = WebApplication.CreateBuilder().Build();
            var routeBuilder = CreateRouteBuilder<UpdateAllowAnonymousEntity>(app, builder =>
            {
                builder.UpdateEndpoint.RequireAuthorization("Policy.Update");
                builder.UpdateEndpoint.AllowAnonymous();
            });

            // Act
            routeBuilder.UpdateEndpoint.Map();

            // Assert
            var endpoint = FindEndpointByName(app, "UpdateUpdateAllowAnonymousEntity");
            var authorizeMetadata = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
            Assert.Empty(authorizeMetadata);
        }

        private static void AssertEndpointHasAuthorizeMetadata<TEntity>(
            Action<SlothEntityBuilder<TEntity>> configure,
            Action<SlothfulEndpointRouteBuilder<TEntity>> mapEndpoint,
            string endpointName,
            string expectedPolicy)
            where TEntity : class, ISlothfulEntity
        {
            // Arrange
            var app = WebApplication.CreateBuilder().Build();
            var routeBuilder = CreateRouteBuilder(app, configure);

            // Act
            mapEndpoint(routeBuilder);

            // Assert
            var endpoint = FindEndpointByName(app, endpointName);
            var authorizeMetadata = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
            Assert.NotEmpty(authorizeMetadata);
            Assert.Contains(authorizeMetadata, m => m.Policy == expectedPolicy);
        }

        private static void AssertEndpointIsNotMapped<TEntity>(
            Action<SlothEntityBuilder<TEntity>> configure,
            Action<SlothfulEndpointRouteBuilder<TEntity>> mapEndpoint)
            where TEntity : class, ISlothfulEntity
        {
            // Arrange
            var app = WebApplication.CreateBuilder().Build();
            var routeBuilder = CreateRouteBuilder(app, configure);
            var endpointsBefore = GetMappedEndpoints(app).Count;

            // Act
            mapEndpoint(routeBuilder);

            // Assert
            var endpointsAfter = GetMappedEndpoints(app).Count;
            Assert.Equal(endpointsBefore, endpointsAfter);
        }

        private static SlothfulEndpointRouteBuilder<TEntity> CreateRouteBuilder<TEntity>(
            WebApplication app,
            Action<SlothEntityBuilder<TEntity>> configure)
            where TEntity : class, ISlothfulEntity
        {
            var configurationBuilder = new SlothEntityBuilder<TEntity>();
            configure(configurationBuilder);

            var parameters = new SlothfulBuilderParams(
                app,
                typeof(FakeDbContext),
                typeof(TEntity),
                new ApiSegmentProvider(),
                new BaseCreateConstructorBehavior());

            return new SlothfulEndpointRouteBuilder<TEntity>(parameters, configurationBuilder);
        }

        private static IReadOnlyList<Endpoint> GetMappedEndpoints(WebApplication app)
        {
            return ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints).ToList();
        }

        private static Endpoint FindEndpointByName(WebApplication app, string endpointName)
        {
            return GetMappedEndpoints(app).Single(endpoint =>
                endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName == endpointName);
        }

        private class FakeDbContext
        {
        }

        private class GetEntity : ISlothfulEntity
        {
            public Guid Id { get; init; }
            public string Name { get; init; } = "Name";
            public string DisplayName => Name;
        }

        private class BrowseEntity : ISlothfulEntity
        {
            public Guid Id { get; init; }
            public string Name { get; init; } = "Name";
            public string DisplayName => Name;
        }

        private class BrowseSelectableEntity : ISlothfulEntity
        {
            public Guid Id { get; init; }
            public string Name { get; init; } = "Name";
            public string DisplayName => Name;
        }

        private class CreateEntity : ISlothfulEntity
        {
            public Guid Id { get; private set; }
            public string Name { get; private set; }
            public string DisplayName => Name;

            public CreateEntity(Guid id, string name)
            {
                Id = id;
                Name = name;
            }
        }

        private class UpdateEntity : ISlothfulEntity
        {
            public Guid Id { get; private set; }
            public string Name { get; private set; }
            public string DisplayName => Name;

            public UpdateEntity(Guid id, string name)
            {
                Id = id;
                Name = name;
            }

            public void Update(string name)
            {
                Name = name;
            }
        }

        private class DeleteEntity : ISlothfulEntity
        {
            public Guid Id { get; init; }
            public string Name { get; init; } = "Name";
            public string DisplayName => Name;
        }

        private class UpdateAllowAnonymousEntity : ISlothfulEntity
        {
            public Guid Id { get; private set; }
            public string Name { get; private set; }
            public string DisplayName => Name;

            public UpdateAllowAnonymousEntity(Guid id, string name)
            {
                Id = id;
                Name = name;
            }

            public void Update(string name)
            {
                Name = name;
            }
        }
    }
}
