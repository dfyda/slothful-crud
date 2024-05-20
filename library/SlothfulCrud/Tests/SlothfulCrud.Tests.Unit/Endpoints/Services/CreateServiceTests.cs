using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Services.Endpoints.Post;
using SlothfulCrud.Providers;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Builders.Endpoints.Behaviors.Constructor;
using SlothfulCrud.Types.Configurations;
using FluentValidation;
using FluentValidation.Results;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Tests.Api.EF;

public class CreateServiceTests
{
    private readonly SlothfulDbContext _dbContext;
    private readonly CreateService<Sloth, SlothfulDbContext> _service;
    private readonly Mock<ICreateConstructorBehavior> _createConstructorBehaviorMock;
    private readonly Mock<IEntityConfigurationProvider> _configurationProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IValidator<Sloth>> _validatorMock;

    public CreateServiceTests()
    {
        var options = new DbContextOptionsBuilder<SlothfulDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _dbContext = new SlothfulDbContext(options);
        _createConstructorBehaviorMock = new Mock<ICreateConstructorBehavior>();
        _configurationProviderMock = new Mock<IEntityConfigurationProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _validatorMock = new Mock<IValidator<Sloth>>();

        _service = new CreateService<Sloth, SlothfulDbContext>(
            _dbContext,
            _createConstructorBehaviorMock.Object,
            _configurationProviderMock.Object
        );
    }

    [Fact]
    public void Create_ShouldAddEntityToDbContext()
    {
        var entityId = Guid.NewGuid();
        var command = new { id = entityId, name = "NewSloth", age = 3 };

        var constructorInfo = typeof(Sloth).GetConstructor(new[] { typeof(Guid), typeof(string), typeof(int) });
        _createConstructorBehaviorMock.Setup(b => b.GetConstructorInfo(typeof(Sloth)))
            .Returns(constructorInfo);

        var configuration = new EntityConfiguration();
        configuration.SetHasValidation(false);
        _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
            .Returns(configuration);

        _serviceScopeMock.Setup(scope => scope.ServiceProvider.GetService(typeof(IValidator<Sloth>)))
            .Returns(_validatorMock.Object);

        _service.Create(entityId, command, _serviceScopeMock.Object);

        var createdEntity = _dbContext.Sloths.Find(entityId);
        Assert.NotNull(createdEntity);
        Assert.Equal(entityId, createdEntity.Id);
        Assert.Equal("NewSloth", createdEntity.Name);
        Assert.Equal(3, createdEntity.Age);
    }

    [Fact]
    public void Create_ShouldValidateEntity_WhenValidationEnabled()
    {
        var entityId = Guid.NewGuid();
        var command = new { id = entityId, name = "NewSloth", age = 3 };

        var constructorInfo = typeof(Sloth).GetConstructor(new[] { typeof(Guid), typeof(string), typeof(int) });
        _createConstructorBehaviorMock.Setup(b => b.GetConstructorInfo(typeof(Sloth)))
            .Returns(constructorInfo);

        var configuration = new EntityConfiguration();
        configuration.SetHasValidation(true);
        _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
            .Returns(configuration);

        _serviceScopeMock.Setup(scope => scope.ServiceProvider.GetService(typeof(IValidator<Sloth>)))
            .Returns(_validatorMock.Object);

        _validatorMock.Setup(v => v.Validate(It.IsAny<IValidationContext>())).Returns(new ValidationResult());

        _service.Create(entityId, command, _serviceScopeMock.Object);

        _validatorMock.Verify(v => v.Validate(It.IsAny<IValidationContext>()), Times.Once);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenConstructorNotFound()
    {
        var entityId = Guid.NewGuid();
        var command = new { id = entityId, name = "NewSloth", age = 3 };

        _createConstructorBehaviorMock.Setup(b => b.GetConstructorInfo(typeof(Sloth)))
            .Returns((System.Reflection.ConstructorInfo)null);

        var configuration = new EntityConfiguration();
        _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
            .Returns(configuration);

        Assert.Throws<ConfigurationException>(() => _service.Create(entityId, command, _serviceScopeMock.Object));
    }

    [Fact]
    public void Create_ShouldInvokeConstructorWithCorrectArguments()
    {
        var entityId = Guid.NewGuid();
        var command = new { id = entityId, name = "NewSloth", age = 3 };

        var constructorInfo = typeof(Sloth).GetConstructor(new[] { typeof(Guid), typeof(string), typeof(int) });
        _createConstructorBehaviorMock.Setup(b => b.GetConstructorInfo(typeof(Sloth)))
            .Returns(constructorInfo);

        var configuration = new EntityConfiguration();
        configuration.SetHasValidation(false);
        _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
            .Returns(configuration);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IValidator<Sloth>)))
            .Returns(_validatorMock.Object);

        _serviceScopeMock.Setup(scope => scope.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        _service.Create(entityId, command, _serviceScopeMock.Object);

        _createConstructorBehaviorMock.Verify(b => b.GetConstructorInfo(typeof(Sloth)), Times.Once);
    }


    [Fact]
    public void Create_ShouldNotValidateEntity_WhenValidationDisabled()
    {
        var entityId = Guid.NewGuid();
        var command = new { id = entityId, name = "NewSloth", age = 3 };

        var constructorInfo = typeof(Sloth).GetConstructor(new[] { typeof(Guid), typeof(string), typeof(int) });
        _createConstructorBehaviorMock.Setup(b => b.GetConstructorInfo(typeof(Sloth)))
            .Returns(constructorInfo);

        var configuration = new EntityConfiguration();
        configuration.SetHasValidation(false);
        _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
            .Returns(configuration);

        _service.Create(entityId, command, _serviceScopeMock.Object);

        _validatorMock.Verify(v => v.Validate(It.IsAny<IValidationContext>()), Times.Never);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenKeyPropertyIsNull()
    {
        var command = new { name = "NewSloth", age = 3 };

        var constructorInfo = typeof(Sloth).GetConstructor(new[] { typeof(Guid), typeof(string), typeof(int) });
        _createConstructorBehaviorMock.Setup(b => b.GetConstructorInfo(typeof(Sloth)))
            .Returns(constructorInfo);

        var configuration = new EntityConfiguration();
        configuration.SetHasValidation(false);
        _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
            .Returns(configuration);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IValidator<Sloth>)))
            .Returns(_validatorMock.Object);

        _serviceScopeMock.Setup(scope => scope.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        Assert.Throws<KeyNotFoundException>(() => _service.Create(null, command, _serviceScopeMock.Object));
    }
    
    [Fact]
    public void Create_ShouldReturnKeyProperty()
    {
        var entityId = Guid.NewGuid();
        var command = new { id = entityId, name = "NewSloth", age = 3 };

        var constructorInfo = typeof(Sloth).GetConstructor(new[] { typeof(Guid), typeof(string), typeof(int) });
        _createConstructorBehaviorMock.Setup(b => b.GetConstructorInfo(typeof(Sloth)))
            .Returns(constructorInfo);

        var configuration = new EntityConfiguration();
        configuration.SetHasValidation(false);
        _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
            .Returns(configuration);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IValidator<Sloth>)))
            .Returns(_validatorMock.Object);

        _serviceScopeMock.Setup(scope => scope.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        var result = _service.Create(entityId, command, _serviceScopeMock.Object);

        Assert.Equal(entityId, result);
    }


    [Fact]
    public void Create_ShouldCallGetConstructorInfo()
    {
        var entityId = Guid.NewGuid();
        var command = new { id = entityId, name = "NewSloth", age = 3 };

        var constructorInfo = typeof(Sloth).GetConstructor(new[] { typeof(Guid), typeof(string), typeof(int) });
        _createConstructorBehaviorMock.Setup(b => b.GetConstructorInfo(typeof(Sloth)))
            .Returns(constructorInfo);

        var configuration = new EntityConfiguration();
        configuration.SetHasValidation(false);
        _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
            .Returns(configuration);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IValidator<Sloth>)))
            .Returns(_validatorMock.Object);

        _serviceScopeMock.Setup(scope => scope.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        _service.Create(entityId, command, _serviceScopeMock.Object);

        _createConstructorBehaviorMock.Verify(b => b.GetConstructorInfo(typeof(Sloth)), Times.Once);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenGetDomainMethodParamFails()
    {
        var entityId = Guid.NewGuid();
        var command = new { id = entityId, name = "NewSloth", age = 3 };

        var constructorInfo = typeof(Sloth).GetConstructor(new[] { typeof(Guid), typeof(string), typeof(int) });
        _createConstructorBehaviorMock.Setup(b => b.GetConstructorInfo(typeof(Sloth)))
            .Returns(constructorInfo);

        var configuration = new EntityConfiguration();
        configuration.SetHasValidation(false);
        _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
            .Returns(configuration);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IValidator<Sloth>)))
            .Returns(_validatorMock.Object);

        _serviceScopeMock.Setup(scope => scope.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        _createConstructorBehaviorMock.Setup(b => b.GetConstructorInfo(typeof(Sloth)))
            .Throws(new ConfigurationException("Error getting constructor info"));

        Assert.Throws<ConfigurationException>(() => _service.Create(entityId, command, _serviceScopeMock.Object));
    }
}
