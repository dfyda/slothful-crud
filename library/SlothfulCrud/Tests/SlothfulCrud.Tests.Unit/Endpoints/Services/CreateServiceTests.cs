using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Providers;
using SlothfulCrud.Services.Endpoints.Post;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Tests.Api.EF;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Tests.Unit.Endpoints.Services
{
    public class CreateServiceTests : IDisposable
    {
        private sealed record CreateSlothCommand(Guid id, string name, int age);
        private sealed record CreateSlothCommandWithoutId(string name, int age);

        private SlothfulDbContext _dbContext;
        private Mock<ICreateConstructorBehavior> _createConstructorBehaviorMock;
        private Mock<IEntityConfigurationProvider> _configurationProviderMock;
        private Mock<IServiceScope> _serviceScopeMock;
        private Mock<IValidator<Sloth>> _validatorMock;

        public CreateServiceTests()
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
                .UseInMemoryDatabase(databaseName: $"CreateServiceTests_{Guid.NewGuid()}")
                .Options;

            _dbContext = new SlothfulDbContext(options);
        }
        
        private void ConfigureMocks()
        {
            _createConstructorBehaviorMock = new Mock<ICreateConstructorBehavior>();

            _configurationProviderMock = new Mock<IEntityConfigurationProvider>();
            var slothEntityConfiguration = new EntityConfiguration();
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(slothEntityConfiguration);
            
            _serviceScopeMock = new Mock<IServiceScope>();
            _validatorMock = new Mock<IValidator<Sloth>>();
        }
        
        private CreateService<Sloth, SlothfulDbContext> GetService()
        {
            return new CreateService<Sloth, SlothfulDbContext>(
                _dbContext,
                _createConstructorBehaviorMock.Object,
                _configurationProviderMock.Object
            );
        }

        private void ConfigureAdditionalMocks(bool hasValidation)
        {
            var constructorInfo = typeof(Sloth).GetConstructor(new[] { typeof(Guid), typeof(string), typeof(int) });
            _createConstructorBehaviorMock.Setup(b => b.GetConstructorInfo(typeof(Sloth)))
                .Returns(constructorInfo);

            var configuration = new EntityConfiguration();
            configuration.SetHasValidation(hasValidation);
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(configuration);
        }

        private static CreateSlothCommand CreateValidCommand(Guid entityId)
        {
            return new CreateSlothCommand(entityId, "NewSloth", 3);
        }

        private static CreateSlothCommandWithoutId CreateCommandWithoutId()
        {
            return new CreateSlothCommandWithoutId("NewSloth", 3);
        }

        private void ArrangeValidatorResolution(IValidator<Sloth> validator = null)
        {
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IValidator<Sloth>)))
                .Returns(validator ?? _validatorMock.Object);
            _serviceScopeMock.Setup(scope => scope.ServiceProvider)
                .Returns(serviceProviderMock.Object);
        }

        [Fact]
        public void Create_ShouldAddEntityToDbContext_WhenCommandIsValid()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var command = CreateValidCommand(entityId);
            ConfigureAdditionalMocks(false);
            ArrangeValidatorResolution();

            // Act
            GetService().Create(entityId, command, _serviceScopeMock.Object);

            // Assert
            var createdEntity = _dbContext.Sloths.Find(entityId);
            Assert.NotNull(createdEntity);
            Assert.Equal(entityId, createdEntity.Id);
            Assert.Equal("NewSloth", createdEntity.Name);
            Assert.Equal(3, createdEntity.Age);
        }

        [Fact]
        public void Create_ShouldValidateEntity_WhenValidationEnabled()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var command = CreateValidCommand(entityId);
            ConfigureAdditionalMocks(true);
            ArrangeValidatorResolution();
            _validatorMock.Setup(v => v.Validate(It.IsAny<IValidationContext>())).Returns(new ValidationResult());

            // Act
            GetService().Create(entityId, command, _serviceScopeMock.Object);

            // Assert
            _validatorMock.Verify(v => v.Validate(It.IsAny<IValidationContext>()), Times.Once);
        }

        [Fact]
        public void Create_ShouldThrowConfigurationException_WhenConstructorIsMissing()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var command = CreateValidCommand(entityId);

            _createConstructorBehaviorMock.Setup(b => b.GetConstructorInfo(typeof(Sloth)))
                .Returns((System.Reflection.ConstructorInfo)null);

            var configuration = new EntityConfiguration();
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(configuration);

            // Act + Assert
            Assert.Throws<ConfigurationException>(() => GetService().Create(entityId, command, _serviceScopeMock.Object));
        }

        [Fact]
        public void Create_ShouldCallConstructorBehavior_WhenCommandIsValid()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var command = CreateValidCommand(entityId);
            ConfigureAdditionalMocks(false);
            ArrangeValidatorResolution();

            // Act
            GetService().Create(entityId, command, _serviceScopeMock.Object);

            // Assert
            _createConstructorBehaviorMock.Verify(b => b.GetConstructorInfo(typeof(Sloth)), Times.Once);
        }


        [Fact]
        public void Create_ShouldNotValidateEntity_WhenValidationDisabled()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var command = CreateValidCommand(entityId);

            ConfigureAdditionalMocks(false);
            ArrangeValidatorResolution();

            // Act
            GetService().Create(entityId, command, _serviceScopeMock.Object);

            // Assert
            _validatorMock.Verify(v => v.Validate(It.IsAny<IValidationContext>()), Times.Never);
        }

        [Fact]
        public void Create_ShouldThrowConfigurationException_WhenKeyIsNull()
        {
            // Arrange
            var command = CreateCommandWithoutId();
            ConfigureAdditionalMocks(false);
            ArrangeValidatorResolution();

            // Act + Assert
            Assert.Throws<ConfigurationException>(() => GetService().Create(null, command, _serviceScopeMock.Object));
        }

        [Fact]
        public void Create_ShouldReturnKeyProperty_WhenCommandIsValid()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var command = CreateValidCommand(entityId);

            ConfigureAdditionalMocks(false);
            ArrangeValidatorResolution();

            // Act
            var result = GetService().Create(entityId, command, _serviceScopeMock.Object);

            // Assert
            Assert.Equal(entityId, result);
        }


        [Fact]
        public void Create_ShouldCallGetConstructorInfo_WhenCommandIsValid()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var command = CreateValidCommand(entityId);

            ConfigureAdditionalMocks(false);
            ArrangeValidatorResolution();

            // Act
            GetService().Create(entityId, command, _serviceScopeMock.Object);

            // Assert
            _createConstructorBehaviorMock.Verify(b => b.GetConstructorInfo(typeof(Sloth)), Times.Once);
        }

        [Fact]
        public void Create_ShouldThrowConfigurationException_WhenConstructorBehaviorFails()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var command = CreateValidCommand(entityId);

            ConfigureAdditionalMocks(false);
            ArrangeValidatorResolution();

            _createConstructorBehaviorMock.Setup(b => b.GetConstructorInfo(typeof(Sloth)))
                .Throws(new ConfigurationException("Error getting constructor info"));

            // Act + Assert
            Assert.Throws<ConfigurationException>(() => GetService().Create(entityId, command, _serviceScopeMock.Object));
        }

        [Fact]
        public void Create_ShouldThrowValidationException_WhenValidationFails()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var command = CreateValidCommand(entityId);
            var validator = new InlineValidator<Sloth>();
            validator.RuleFor(x => x.Name).Equal("ValidName");

            ConfigureAdditionalMocks(true);
            ArrangeValidatorResolution(validator);

            // Act + Assert
            Assert.Throws<ValidationException>(() => GetService().Create(entityId, command, _serviceScopeMock.Object));
        }
    }
}