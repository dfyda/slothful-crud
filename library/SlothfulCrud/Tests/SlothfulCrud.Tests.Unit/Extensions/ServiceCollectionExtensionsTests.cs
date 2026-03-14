using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Domain;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Exceptions.Handlers;
using SlothfulCrud.Extensions;
using SlothfulCrud.Managers;
using SlothfulCrud.Providers;
using SlothfulCrud.Services;
using SlothfulCrud.Services.Endpoints.Delete;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Services.Endpoints.Post;
using SlothfulCrud.Services.Endpoints.Put;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Tests.Api.EF;
using SlothfulCrud.Types.Configurations;
using System.Reflection;

namespace SlothfulCrud.Tests.Unit.Extensions
{
    public class ServiceCollectionExtensionsTests
    {
        private const string DynamicServicesCollectionMethodName = "GetDynamicServicesCollection";
        private const string DynamicProvidersCollectionMethodName = "GetDynamicProvidersCollection";
        private const string InMemoryDatabaseNamePrefix = "ServiceCollectionExtensionsTests";
        private const string TestDisplayName = "Test";
        private const string LegacyAddMethodName = "AddSlothfulCrud";
        private const string ObsoleteAttributeName = "ObsoleteAttribute";

        [Fact]
        public void AddSlothfulCrud_ShouldRegisterInfrastructureServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<SlothfulDbContext>(options => options.UseInMemoryDatabase($"{InMemoryDatabaseNamePrefix}_{Guid.NewGuid()}"));

            // Act
            services.AddSlothfulCrud<SlothfulDbContext>();

            // Assert
            AssertContainsService<ISlothfulCrudManager, SlothfulCrudManager>(services, ServiceLifetime.Scoped);
            AssertContainsService<IApiSegmentProvider, ApiSegmentProvider>(services, ServiceLifetime.Scoped);
            AssertContainsService<IEntityConfigurationProvider, EntityConfigurationProvider>(services, ServiceLifetime.Singleton);
            AssertContainsService<IExceptionHandler, ExceptionHandler>(services, ServiceLifetime.Transient);
            AssertContainsService<ICreateConstructorBehavior, BaseCreateConstructorBehavior>(services, ServiceLifetime.Scoped);
        }

        [Fact]
        public void AddSlothfulCrudWithMarker_ShouldRegisterInfrastructureServices()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<SlothfulDbContext>(options => options.UseInMemoryDatabase($"{InMemoryDatabaseNamePrefix}_{Guid.NewGuid()}"));

            // Act
            services.AddSlothfulCrud<SlothfulDbContext, Sloth>();

            // Assert
            AssertContainsService<ISlothfulCrudManager, SlothfulCrudManager>(services, ServiceLifetime.Scoped);
            AssertContainsService<IApiSegmentProvider, ApiSegmentProvider>(services, ServiceLifetime.Scoped);
            AssertContainsService<IEntityConfigurationProvider, EntityConfigurationProvider>(services, ServiceLifetime.Singleton);
            AssertContainsService<IExceptionHandler, ExceptionHandler>(services, ServiceLifetime.Transient);
            AssertContainsService<ICreateConstructorBehavior, BaseCreateConstructorBehavior>(services, ServiceLifetime.Scoped);
        }

        [Fact]
        public void AddSlothfulCrudWithMarker_ShouldRegisterSlothfulOptionsAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<SlothfulDbContext>(options => options.UseInMemoryDatabase($"{InMemoryDatabaseNamePrefix}_{Guid.NewGuid()}"));

            // Act
            services.AddSlothfulCrud<SlothfulDbContext, Sloth>();

            // Assert
            var descriptor = Assert.Single(services, d => d.ServiceType == typeof(SlothfulOptions));
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        [Fact]
        public void AddSlothfulCrudWithMarker_ShouldInvokeConfigureOptionsCallback()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<SlothfulDbContext>(options => options.UseInMemoryDatabase($"{InMemoryDatabaseNamePrefix}_{Guid.NewGuid()}"));

            // Act
            services.AddSlothfulCrud<SlothfulDbContext, Sloth>(opts =>
            {
                opts.UseSlothfulProblemHandling = true;
            });

            // Assert
            var provider = services.BuildServiceProvider();
            var registeredOptions = provider.GetRequiredService<SlothfulOptions>();
            Assert.True(registeredOptions.UseSlothfulProblemHandling);
        }

        [Fact]
        public void AddSlothfulCrudLegacy_ShouldHaveObsoleteAttribute()
        {
            var method = typeof(ServiceCollectionExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == LegacyAddMethodName && m.GetGenericArguments().Length == 1);

            Assert.True(method.IsDefined(typeof(ObsoleteAttribute), false));
        }

        [Fact]
        public void GetDynamicServicesCollection_ShouldContainCrudContracts_ForEntityType()
        {
            // Arrange
            var method = typeof(ServiceCollectionExtensions)
                .GetMethod(DynamicServicesCollectionMethodName, BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(SlothfulDbContext));

            // Act
            var result = (Dictionary<Type, Type>)method.Invoke(null, [typeof(TestEntity)])!;

            // Assert
            Assert.Contains(typeof(IEndpointsService<TestEntity, SlothfulDbContext>), result.Keys);
            Assert.Contains(typeof(IGetService<TestEntity, SlothfulDbContext>), result.Keys);
            Assert.Contains(typeof(IBrowseService<TestEntity, SlothfulDbContext>), result.Keys);
            Assert.Contains(typeof(IBrowseSelectableService<TestEntity, SlothfulDbContext>), result.Keys);
            Assert.Contains(typeof(ICreateService<TestEntity, SlothfulDbContext>), result.Keys);
            Assert.Contains(typeof(IUpdateService<TestEntity, SlothfulDbContext>), result.Keys);
            Assert.Contains(typeof(IDeleteService<TestEntity, SlothfulDbContext>), result.Keys);
        }

        [Fact]
        public void GetDynamicProvidersCollection_ShouldContainKeyValueProviderContract_ForEntityType()
        {
            // Arrange
            var method = typeof(ServiceCollectionExtensions)
                .GetMethod(DynamicProvidersCollectionMethodName, BindingFlags.NonPublic | BindingFlags.Static)!;

            // Act
            var result = (Dictionary<Type, Type>)method.Invoke(null, [typeof(TestEntity)])!;

            // Assert
            Assert.Contains(typeof(IEntityPropertyKeyValueProvider<TestEntity>), result.Keys);
            Assert.Equal(typeof(EntityPropertyKeyValueProvider<TestEntity>), result[typeof(IEntityPropertyKeyValueProvider<TestEntity>)]);
        }

        private static void AssertContainsService<TService, TImplementation>(
            IServiceCollection services,
            ServiceLifetime lifetime)
        {
            Assert.Contains(services, descriptor =>
                descriptor.ServiceType == typeof(TService)
                && descriptor.ImplementationType == typeof(TImplementation)
                && descriptor.Lifetime == lifetime);
        }

        private class TestEntity : ISlothfulEntity
        {
            public string DisplayName => TestDisplayName;
        }
    }
}
