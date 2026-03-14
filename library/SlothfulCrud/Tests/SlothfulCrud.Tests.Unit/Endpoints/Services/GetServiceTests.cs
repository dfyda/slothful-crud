using Microsoft.EntityFrameworkCore;
using Moq;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Providers;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Tests.Api.EF;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Tests.Unit.Endpoints.Services
{
    public class GetServiceTests : IDisposable
    {
        private const string InvalidIdValue = "invalid_id";
        private const string TestName = "Test";
        private const string FirstEntityName = "First";
        private const string SecondEntityName = "Second";
        private const string CuisineSlothName = "CuisineSloth";
        private const string NeighbourSlothName = "NeighbourSloth";
        private const string SpeedyKoalaName = "SpeedyKoala";
        private const string UnknownKeyPropertyName = "UnknownKey";
        private const int SlothAge = 5;
        private const int SecondSlothAge = 6;
        private const int CuisineAge = 3;
        private const int NeighbourAge = 4;

        private SlothfulDbContext _dbContext;
        private Mock<IEntityConfigurationProvider> _configurationProviderMock;

        public GetServiceTests()
        {
            ConfigureDbContext();
            ConfigureMocks();
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
        
        private void ConfigureDbContext()
        {
            var options = new DbContextOptionsBuilder<SlothfulDbContext>()
                .UseInMemoryDatabase(databaseName: $"GetServiceTests_{Guid.NewGuid()}")
                .Options;

            _dbContext = new SlothfulDbContext(options);
        }
        
        private void ConfigureMocks()
        {
            _configurationProviderMock = new Mock<IEntityConfigurationProvider>();
            
            var slothEntityConfiguration = new EntityConfiguration();
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(slothEntityConfiguration);

            var wildKoalaEntityConfiguration = new EntityConfiguration();
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(WildKoala)))
                .Returns(wildKoalaEntityConfiguration);
        }
        
        private GetService<Sloth, SlothfulDbContext> GetSlothService()
        {
            return new GetService<Sloth, SlothfulDbContext>(_dbContext, _configurationProviderMock.Object, new SlothfulOptions());
        }
        
        private GetService<WildKoala, SlothfulDbContext> GetWildKoalaService()
        {
            return new GetService<WildKoala, SlothfulDbContext>(_dbContext, _configurationProviderMock.Object, new SlothfulOptions());
        }

        [Fact]
        public async Task Get_ShouldReturnSloth_WhenEntityExists()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var entity = new Sloth(entityId, TestName, SlothAge);

            _dbContext.Sloths.Add(entity);
            _dbContext.SaveChanges();

            // Act
            var result = await GetSlothService().GetAsync(entity.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Id);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Age, result.Age);
        }

        [Fact]
        public async Task Get_ShouldReturnWildKoala_WhenEntityExists()
        {
            // Arrange
            var cuisine = new Sloth(Guid.NewGuid(), CuisineSlothName, CuisineAge);
            var neighbour = new Sloth(Guid.NewGuid(), NeighbourSlothName, NeighbourAge);
            var entityId = Guid.NewGuid();
            var entity = new WildKoala(entityId, SpeedyKoalaName, SlothAge, cuisine, neighbour);

            _dbContext.Sloths.Add(cuisine);
            _dbContext.Sloths.Add(neighbour);
            _dbContext.Koalas.Add(entity);
            _dbContext.SaveChanges();

            // Act
            var result = await GetWildKoalaService().GetAsync(entity.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Id);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Age, result.Age);
            Assert.Equal(cuisine.Id, result.CuisineId);
            Assert.Equal(neighbour.Id, result.NeighbourId);
        }

        [Fact]
        public async Task Get_ShouldThrowNotFoundException_WhenEntityDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act + Assert
            await Assert.ThrowsAsync<NotFoundException>(() => GetSlothService().GetAsync(nonExistentId));
            await Assert.ThrowsAsync<NotFoundException>(() => GetWildKoalaService().GetAsync(nonExistentId));
        }

        [Fact]
        public async Task Get_ShouldIncludeDependencies_WhenEntityIsWildKoala()
        {
            // Arrange
            var cuisine = new Sloth(Guid.NewGuid(), CuisineSlothName, CuisineAge);
            var neighbour = new Sloth(Guid.NewGuid(), NeighbourSlothName, NeighbourAge);
            var entityId = Guid.NewGuid();
            var entity = new WildKoala(entityId, SpeedyKoalaName, SlothAge, cuisine, neighbour);

            _dbContext.Sloths.Add(cuisine);
            _dbContext.Sloths.Add(neighbour);
            _dbContext.Koalas.Add(entity);
            _dbContext.SaveChanges();

            // Act
            var result = await GetWildKoalaService().GetAsync(entity.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cuisine.Name, result.Cuisine.Name);
            Assert.Equal(neighbour.Name, result.Neighbour.Name);
        }

        [Fact]
        public async Task Get_ShouldThrowConfigurationException_WhenIdTypeIsInvalid()
        {
            // Arrange
            var invalidId = InvalidIdValue;

            // Act + Assert
            await Assert.ThrowsAsync<ConfigurationException>(() => GetSlothService().GetAsync(invalidId));
        }

        [Fact]
        public async Task Get_ShouldThrowConfigurationException_WhenEntityConfigurationIsMissing()
        {
            // Arrange
            var nonConfiguredService =
                new GetService<WildKoala, SlothfulDbContext>(_dbContext,
                    new Mock<IEntityConfigurationProvider>().Object, new SlothfulOptions());
            var entityId = Guid.NewGuid();

            // Act + Assert
            await Assert.ThrowsAsync<ConfigurationException>(() => nonConfiguredService.GetAsync(entityId));
        }

        [Fact]
        public async Task Get_ShouldThrowConfigurationException_WhenIdIsNull()
        {
            // Act + Assert
            await Assert.ThrowsAsync<ConfigurationException>(() => GetSlothService().GetAsync(null));
        }

        [Fact]
        public async Task Get_ShouldReturnEntity_WhenNonDefaultKeyPropertyAndTypeAreConfigured()
        {
            // Arrange
            var firstEntity = new Sloth(Guid.NewGuid(), FirstEntityName, SlothAge);
            var secondEntity = new Sloth(Guid.NewGuid(), SecondEntityName, SecondSlothAge);
            _dbContext.Sloths.AddRange(firstEntity, secondEntity);
            _dbContext.SaveChanges();

            var configuration = new EntityConfiguration();
            configuration.SetKeyProperty(nameof(Sloth.Age));
            configuration.SetKeyPropertyType(typeof(int));
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(configuration);

            // Act
            var result = await GetSlothService().GetAsync(SecondSlothAge);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(secondEntity.Id, result.Id);
            Assert.Equal(SecondEntityName, result.Name);
            Assert.Equal(SecondSlothAge, result.Age);
        }

        [Fact]
        public async Task Get_ShouldThrowConfigurationException_WhenConfiguredKeyPropertyDoesNotExist()
        {
            // Arrange
            var configuration = new EntityConfiguration();
            configuration.SetKeyProperty(UnknownKeyPropertyName);
            configuration.SetKeyPropertyType(typeof(Guid));
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(configuration);

            // Act + Assert
            await Assert.ThrowsAsync<ConfigurationException>(() => GetSlothService().GetAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task Get_ShouldReturnEntity_WhenQueryCustomizerIsNull()
        {
            // Arrange
            var entity = new Sloth(Guid.NewGuid(), TestName, SlothAge);
            _dbContext.Sloths.Add(entity);
            _dbContext.SaveChanges();

            var options = new SlothfulOptions { QueryCustomizer = null };
            var service = new GetService<Sloth, SlothfulDbContext>(_dbContext, _configurationProviderMock.Object, options);

            // Act
            var result = await service.GetAsync(entity.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Id);
        }

        [Fact]
        public async Task Get_ShouldReturnEntity_WhenQueryCustomizerIsNoOp()
        {
            // Arrange
            var entity = new Sloth(Guid.NewGuid(), TestName, SlothAge);
            _dbContext.Sloths.Add(entity);
            _dbContext.SaveChanges();

            var options = new SlothfulOptions { QueryCustomizer = q => q };
            var service = new GetService<Sloth, SlothfulDbContext>(_dbContext, _configurationProviderMock.Object, options);

            // Act
            var result = await service.GetAsync(entity.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Id);
        }

        [Fact]
        public async Task Get_ShouldThrowInvalidOperationException_WhenQueryCustomizerReturnsInvalidType()
        {
            // Arrange
            var entity = new Sloth(Guid.NewGuid(), TestName, SlothAge);
            _dbContext.Sloths.Add(entity);
            _dbContext.SaveChanges();

            var options = new SlothfulOptions
            {
                QueryCustomizer = q => new List<string>().AsQueryable()
            };
            var service = new GetService<Sloth, SlothfulDbContext>(_dbContext, _configurationProviderMock.Object, options);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetAsync(entity.Id));
        }
    }
}
