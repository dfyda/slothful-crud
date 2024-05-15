using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SlothfulCrud.Services.Endpoints.Put;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Providers;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Types.Configurations;
using FluentValidation;
using FluentValidation.Results;
using SlothfulCrud.Tests.Api.EF;

public class UpdateServiceTests
{
    private readonly SlothfulDbContext _dbContext;
    private readonly UpdateService<Sloth, SlothfulDbContext> _service;
    private readonly Mock<IGetService<Sloth, SlothfulDbContext>> _getServiceMock;
    private readonly Mock<IEntityConfigurationProvider> _configurationProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IValidator<Sloth>> _validatorMock;

    public UpdateServiceTests()
    {
        var options = new DbContextOptionsBuilder<SlothfulDbContext>()
            .UseInMemoryDatabase(databaseName: "TestUpdateDatabase")
            .Options;

        _dbContext = new SlothfulDbContext(options);
        _getServiceMock = new Mock<IGetService<Sloth, SlothfulDbContext>>();
        _configurationProviderMock = new Mock<IEntityConfigurationProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _validatorMock = new Mock<IValidator<Sloth>>();

        _service = new UpdateService<Sloth, SlothfulDbContext>(
            _dbContext,
            _getServiceMock.Object,
            _configurationProviderMock.Object
        );
    }

    [Fact]
    public void Update_ShouldUpdateEntityInDbContext()
    {
        var entityId = Guid.NewGuid();
        var entity = new Sloth(entityId, "OldSloth", 5);
        var command = new { name = "UpdatedSloth", age = 10 };

        var configuration = new EntityConfiguration();
        _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
            .Returns(configuration);

        _serviceScopeMock.Setup(scope => scope.ServiceProvider.GetService(typeof(IValidator<Sloth>)))
            .Returns(_validatorMock.Object);

        _dbContext.Sloths.Add(entity);
        _dbContext.SaveChanges();

        _getServiceMock.Setup(s => s.Get(entityId)).Returns(entity);

        _service.Update(entityId, command, _serviceScopeMock.Object);

        var updatedEntity = _dbContext.Sloths.Find(entityId);
        Assert.NotNull(updatedEntity);
        Assert.Equal("UpdatedSloth", updatedEntity.Name);
        Assert.Equal(10, updatedEntity.Age);
    }

    [Fact]
    public void Update_ShouldValidateEntity_WhenValidationEnabled()
    {
        var entityId = Guid.NewGuid();
        var entity = new Sloth(entityId, "OldSloth", 5);
        var command = new { name = "UpdatedSloth", age = 10 };

        var configuration = new EntityConfiguration();
        configuration.SetHasValidation(true);
        _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
            .Returns(configuration);

        _serviceScopeMock.Setup(scope => scope.ServiceProvider.GetService(typeof(IValidator<Sloth>)))
            .Returns(_validatorMock.Object);

        _validatorMock.Setup(v => v.Validate(It.IsAny<IValidationContext>())).Returns(new ValidationResult());

        _dbContext.Sloths.Add(entity);
        _dbContext.SaveChanges();

        _getServiceMock.Setup(s => s.Get(entityId)).Returns(entity);

        _service.Update(entityId, command, _serviceScopeMock.Object);

        _validatorMock.Verify(v => v.Validate(It.IsAny<IValidationContext>()), Times.Once);
    }
}
