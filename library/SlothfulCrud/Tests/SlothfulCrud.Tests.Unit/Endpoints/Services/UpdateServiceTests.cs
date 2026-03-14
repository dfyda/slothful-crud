using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Providers;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Services.Endpoints.Put;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Tests.Api.EF;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Tests.Unit.Endpoints.Services
{
    public class UpdateServiceTests : IDisposable
    {
        private const string InvalidIdValue = "invalid_id";
        private const string UpdateMethodName = "Update";
        private const string NotExistingMethodName = "NotExistingMethod";
        private const string OldSlothName = "OldSloth";
        private const string OldKoalaName = "OldKoala";
        private const string UpdatedSlothName = "UpdatedSloth";
        private const string UpdatedKoalaName = "UpdatedKoala";
        private const string CuisineSlothName = "CuisineSloth";
        private const string NeighbourSlothName = "NeighbourSloth";
        private const string ValidName = "ValidName";
        private const int InitialAge = 5;
        private const int UpdatedAge = 10;
        private const int CuisineAge = 3;
        private const int NeighbourAge = 4;

        private sealed record UpdateSlothCommand(string name, int age);
        private sealed record UpdateWildKoalaCommand(string name, int age, int? age2, Guid cuisineId, Guid neighbourId);

        private SlothfulDbContext _dbContext;
        private Mock<IGetService<Sloth, SlothfulDbContext>> _slothGetServiceMock;
        private Mock<IGetService<WildKoala, SlothfulDbContext>> _wildKoalaGetServiceMock;
        private Mock<IEntityConfigurationProvider> _configurationProviderMock;
        private Mock<IServiceScope> _serviceScopeMock;
        private Mock<IValidator<Sloth>> _slothValidatorMock;
        private Mock<IValidator<WildKoala>> _wildKoalaValidatorMock;

        public UpdateServiceTests()
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
                .UseInMemoryDatabase(databaseName: $"UpdateServiceTests_{Guid.NewGuid()}")
                .Options;

            _dbContext = new SlothfulDbContext(options);
        }

        private void ConfigureMocks()
        {
            _slothGetServiceMock = new Mock<IGetService<Sloth, SlothfulDbContext>>();
            _wildKoalaGetServiceMock = new Mock<IGetService<WildKoala, SlothfulDbContext>>();
            _configurationProviderMock = new Mock<IEntityConfigurationProvider>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _slothValidatorMock = new Mock<IValidator<Sloth>>();
            _wildKoalaValidatorMock = new Mock<IValidator<WildKoala>>();
            
            var wildKoalaEntityConfiguration = new EntityConfiguration();
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(WildKoala)))
                .Returns(wildKoalaEntityConfiguration);
        }

        private void ConfigureAdditionalMocks()
        {
            var configuration = new EntityConfiguration();
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(configuration);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IValidator<Sloth>)))
                .Returns(_slothValidatorMock.Object);

            _serviceScopeMock.Setup(scope => scope.ServiceProvider)
                .Returns(serviceProviderMock.Object);
            
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(WildKoala)))
                .Returns(configuration);
            
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IValidator<WildKoala>)))
                .Returns(_wildKoalaValidatorMock.Object);

            _serviceScopeMock.Setup(scope => scope.ServiceProvider)
                .Returns(serviceProviderMock.Object);
        }

        private static UpdateSlothCommand CreateSlothUpdateCommand()
        {
            return new UpdateSlothCommand(UpdatedSlothName, UpdatedAge);
        }

        private static UpdateWildKoalaCommand CreateWildKoalaUpdateCommand(Guid cuisineId, Guid neighbourId)
        {
            return new UpdateWildKoalaCommand(UpdatedKoalaName, UpdatedAge, null, cuisineId, neighbourId);
        }

        private void ArrangeSlothValidator(IValidator<Sloth> validator = null)
        {
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IValidator<Sloth>)))
                .Returns(validator ?? _slothValidatorMock.Object);
            _serviceScopeMock.Setup(scope => scope.ServiceProvider)
                .Returns(serviceProviderMock.Object);
        }
        
        private UpdateService<Sloth, SlothfulDbContext> GetSlothService()
        {
            return new UpdateService<Sloth, SlothfulDbContext>(
                _dbContext,
                _slothGetServiceMock.Object,
                _configurationProviderMock.Object,
                new SlothfulOptions()
            );
        }
        
        private UpdateService<WildKoala, SlothfulDbContext> GetWildKoalaService()
        {
            return new UpdateService<WildKoala, SlothfulDbContext>(
                _dbContext,
                _wildKoalaGetServiceMock.Object,
                _configurationProviderMock.Object,
                new SlothfulOptions()
            );
        }

        [Fact]
        public async Task Update_ShouldModifySloth_WhenCommandIsValid()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var entity = new Sloth(entityId, OldSlothName, InitialAge);
            var command = CreateSlothUpdateCommand();

            ConfigureAdditionalMocks();

            _dbContext.Sloths.Add(entity);
            _dbContext.SaveChanges();

            _slothGetServiceMock.Setup(s => s.GetAsync(entityId)).ReturnsAsync(entity);

            // Act
            await GetSlothService().UpdateAsync(entityId, command, _serviceScopeMock.Object);

            // Assert
            var updatedEntity = _dbContext.Sloths.Find(entityId);
            Assert.NotNull(updatedEntity);
            Assert.Equal(UpdatedSlothName, updatedEntity.Name);
            Assert.Equal(UpdatedAge, updatedEntity.Age);
        }

        [Fact]
        public async Task Update_ShouldValidateEntity_WhenValidationEnabled()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var entity = new Sloth(entityId, OldSlothName, InitialAge);
            var command = CreateSlothUpdateCommand();

            ConfigureAdditionalMocks();

            _slothValidatorMock.Setup(v => v.Validate(It.IsAny<IValidationContext>())).Returns(new ValidationResult());

            _dbContext.Sloths.Add(entity);
            _dbContext.SaveChanges();

            _slothGetServiceMock.Setup(s => s.GetAsync(entityId)).ReturnsAsync(entity);

            // Act
            await GetSlothService().UpdateAsync(entityId, command, _serviceScopeMock.Object);

            // Assert
            _slothValidatorMock.Verify(v => v.Validate(It.IsAny<IValidationContext>()), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldModifyWildKoala_WhenCommandIsValid()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var cuisine = new Sloth(Guid.NewGuid(), CuisineSlothName, CuisineAge);
            var neighbour = new Sloth(Guid.NewGuid(), NeighbourSlothName, NeighbourAge);
            var entity = new WildKoala(entityId, OldKoalaName, InitialAge, cuisine, neighbour);
            var cuisineId = cuisine.Id;
            var neighbourId = neighbour.Id;
            var command = CreateWildKoalaUpdateCommand(cuisineId, neighbourId);

            ConfigureAdditionalMocks();

            _dbContext.Sloths.Add(cuisine);
            _dbContext.Sloths.Add(neighbour);
            _dbContext.Koalas.Add(entity);
            _dbContext.SaveChanges();

            _wildKoalaGetServiceMock.Setup(s => s.GetAsync(entityId)).ReturnsAsync(entity);

            // Act
            await GetWildKoalaService().UpdateAsync(entityId, command, _serviceScopeMock.Object);

            // Assert
            var updatedEntity = _dbContext.Koalas.Find(entityId);
            Assert.NotNull(updatedEntity);
            Assert.Equal(UpdatedKoalaName, updatedEntity.Name);
            Assert.Equal(UpdatedAge, updatedEntity.Age);
            Assert.Null(updatedEntity.Age2);
            Assert.Equal(cuisine.Id, updatedEntity.CuisineId);
            Assert.Equal(neighbour.Id, updatedEntity.NeighbourId);
        }

        [Fact]
        public async Task Update_ShouldValidateWildKoala_WhenValidationEnabled()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var cuisine = new Sloth(Guid.NewGuid(), CuisineSlothName, CuisineAge);
            var neighbour = new Sloth(Guid.NewGuid(), NeighbourSlothName, NeighbourAge);
            var entity = new WildKoala(entityId, OldKoalaName, InitialAge, cuisine, neighbour);
            var cuisineId = cuisine.Id;
            var neighbourId = neighbour.Id;
            var command = CreateWildKoalaUpdateCommand(cuisineId, neighbourId);

            ConfigureAdditionalMocks();

            _dbContext.Sloths.Add(cuisine);
            _dbContext.Sloths.Add(neighbour);
            _dbContext.Koalas.Add(entity);
            _dbContext.SaveChanges();

            _wildKoalaGetServiceMock.Setup(s => s.GetAsync(entityId)).ReturnsAsync(entity);

            // Act
            await GetWildKoalaService().UpdateAsync(entityId, command, _serviceScopeMock.Object);

            // Assert
            _wildKoalaValidatorMock.Verify(v => v.Validate(It.IsAny<IValidationContext>()), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldThrowConfigurationException_WhenIdTypeIsInvalid()
        {
            // Arrange
            var invalidId = InvalidIdValue;
            var command = CreateSlothUpdateCommand();

            var configuration = new EntityConfiguration();
            configuration.SetUpdateMethod(UpdateMethodName);
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(configuration);

            // Act + Assert
            await Assert.ThrowsAsync<ConfigurationException>(() =>
                GetSlothService().UpdateAsync(invalidId, command, _serviceScopeMock.Object));
        }

        [Fact]
        public async Task Update_ShouldThrowConfigurationException_WhenEntityConfigurationIsMissing()
        {
            // Arrange
            var nonConfiguredService = new UpdateService<Sloth, SlothfulDbContext>(_dbContext,
                _slothGetServiceMock.Object, new Mock<IEntityConfigurationProvider>().Object, new SlothfulOptions());
            var entityId = Guid.NewGuid();
            var command = CreateSlothUpdateCommand();

            // Act + Assert
            await Assert.ThrowsAsync<ConfigurationException>(() =>
                nonConfiguredService.UpdateAsync(entityId, command, _serviceScopeMock.Object));
        }

        [Fact]
        public async Task Update_ShouldThrowConfigurationException_WhenIdIsNull()
        {
            // Arrange
            var command = CreateSlothUpdateCommand();

            var configuration = new EntityConfiguration();
            configuration.SetUpdateMethod(UpdateMethodName);
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(configuration);

            // Act + Assert
            await Assert.ThrowsAsync<ConfigurationException>(() => GetSlothService().UpdateAsync(null, command, _serviceScopeMock.Object));
        }

        [Fact]
        public async Task Update_ShouldThrowConfigurationException_WhenUpdateMethodIsMissing()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var entity = new Sloth(entityId, OldSlothName, InitialAge);
            var command = CreateSlothUpdateCommand();

            var configuration = new EntityConfiguration();
            configuration.SetUpdateMethod(NotExistingMethodName);
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(configuration);

            _dbContext.Sloths.Add(entity);
            _dbContext.SaveChanges();

            _slothGetServiceMock.Setup(s => s.GetAsync(entityId)).ReturnsAsync(entity);

            // Act + Assert
            await Assert.ThrowsAsync<ConfigurationException>(() =>
                GetSlothService().UpdateAsync(entityId, command, _serviceScopeMock.Object));
        }

        [Fact]
        public async Task Update_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var entity = new Sloth(entityId, OldSlothName, InitialAge);
            var command = CreateSlothUpdateCommand();
            var validator = new InlineValidator<Sloth>();
            validator.RuleFor(x => x.Name).Equal(ValidName);

            ConfigureAdditionalMocks();
            ArrangeSlothValidator(validator);

            _dbContext.Sloths.Add(entity);
            _dbContext.SaveChanges();

            _slothGetServiceMock.Setup(s => s.GetAsync(entityId)).ReturnsAsync(entity);

            // Act + Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                GetSlothService().UpdateAsync(entityId, command, _serviceScopeMock.Object));
        }
    }
}
