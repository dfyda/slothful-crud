using Moq;
using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Providers;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Types.Configurations;
using SlothfulCrud.Tests.Api.EF;

public class GetServiceTests
{
    private readonly SlothfulDbContext _dbContext;
    private readonly GetService<Sloth, SlothfulDbContext> _service;

    public GetServiceTests()
    {
        var options = new DbContextOptionsBuilder<SlothfulDbContext>()
            .UseInMemoryDatabase(databaseName: "TestGetDatabase")
            .Options;

        _dbContext = new SlothfulDbContext(options);
        var configurationProviderMock = new Mock<IEntityConfigurationProvider>();
        var entityConfiguration = new EntityConfiguration();
        configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
            .Returns(entityConfiguration);
        
        _service = new GetService<Sloth, SlothfulDbContext>(_dbContext, configurationProviderMock.Object);
    }

    [Fact]
    public void Get_ShouldReturnEntity()
    {
        var entityId = Guid.NewGuid();
        var entity = new Sloth(entityId, "Test", 5);

        _dbContext.Sloths.Add(entity);
        _dbContext.SaveChanges();

        var result = _service.Get(entity.Id);

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Name, result.Name);
        Assert.Equal(entity.Age, result.Age);
    }
}