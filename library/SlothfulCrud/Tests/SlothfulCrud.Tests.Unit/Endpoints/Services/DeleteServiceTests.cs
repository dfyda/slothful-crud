using Microsoft.EntityFrameworkCore;
using Moq;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Providers;
using SlothfulCrud.Services.Endpoints.Delete;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Tests.Api.EF;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Tests.Unit.Endpoints.Services
{
    public class DeleteServiceTests : IDisposable
    {
        private const string TestName = "Test";
        private const int TestAge = 5;

        private SlothfulDbContext _dbContext;
        private Mock<IGetService<Sloth, SlothfulDbContext>> _getServiceMock;
        private Mock<IEntityConfigurationProvider> _configurationProviderMock;

        public DeleteServiceTests()
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
                .UseInMemoryDatabase(databaseName: $"DeleteServiceTests_{Guid.NewGuid()}")
                .Options;

            _dbContext = new SlothfulDbContext(options);
        }
        
        private void ConfigureMocks()
        {
            _getServiceMock = new Mock<IGetService<Sloth, SlothfulDbContext>>();
            
            _configurationProviderMock = new Mock<IEntityConfigurationProvider>();
            var slothEntityConfiguration = new EntityConfiguration();
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(slothEntityConfiguration);
        }

        private DeleteService<Sloth, SlothfulDbContext> GetService()
        {
            return new DeleteService<Sloth, SlothfulDbContext>(_dbContext, _getServiceMock.Object,
                _configurationProviderMock.Object, new SlothfulOptions());
        }

        private Sloth SeedSloth(Guid entityId)
        {
            var entity = new Sloth(entityId, TestName, TestAge);
            _dbContext.Sloths.Add(entity);
            _dbContext.SaveChanges();
            return entity;
        }
        
        [Fact]
        public async Task Delete_ShouldRemoveEntity_WhenEntityExists()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var entity = SeedSloth(entityId);

            _getServiceMock.Setup(s => s.GetAsync(entityId)).ReturnsAsync(entity);

            // Act
            await GetService().DeleteAsync(entityId);

            // Assert
            var deletedEntity = _dbContext.Sloths.Find(entityId);
            Assert.Null(deletedEntity);
        }

        [Fact]
        public async Task Delete_ShouldThrowNotFoundException_WhenEntityDoesNotExist()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            _getServiceMock.Setup(s => s.GetAsync(entityId)).ThrowsAsync(new NotFoundException());

            // Act + Assert
            await Assert.ThrowsAsync<NotFoundException>(() => GetService().DeleteAsync(entityId));
        }

        [Fact]
        public async Task Delete_ShouldCallGetService_WhenEntityExists()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var entity = SeedSloth(entityId);

            _getServiceMock.Setup(s => s.GetAsync(entityId)).ReturnsAsync(entity);

            // Act
            await GetService().DeleteAsync(entityId);

            // Assert
            _getServiceMock.Verify(s => s.GetAsync(entityId), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldThrowConfigurationException_WhenKeyIsNull()
        {
            // Arrange
            _getServiceMock.Setup(s => s.GetAsync(null)).ThrowsAsync(new ConfigurationException());

            // Act + Assert
            await Assert.ThrowsAsync<ConfigurationException>(() => GetService().DeleteAsync(null));
        }
    }
}
