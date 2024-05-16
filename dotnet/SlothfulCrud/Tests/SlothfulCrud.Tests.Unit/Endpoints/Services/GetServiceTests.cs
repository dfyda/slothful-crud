using Moq;
using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Providers;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Types.Configurations;
using SlothfulCrud.Tests.Api.EF;

public class GetServiceTests
{
    private readonly SlothfulDbContext _dbContext;
    private readonly GetService<Sloth, SlothfulDbContext> _slothService;
    private readonly GetService<WildKoala, SlothfulDbContext> _wildKoalaService;

    public GetServiceTests()
    {
        var options = new DbContextOptionsBuilder<SlothfulDbContext>()
            .UseInMemoryDatabase(databaseName: "TestGetDatabase")
            .Options;

        _dbContext = new SlothfulDbContext(options);
        var configurationProviderMock = new Mock<IEntityConfigurationProvider>();
        var slothEntityConfiguration = new EntityConfiguration();
        configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
            .Returns(slothEntityConfiguration);

        var wildKoalaEntityConfiguration = new EntityConfiguration();
        configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(WildKoala)))
            .Returns(wildKoalaEntityConfiguration);
        
        _slothService = new GetService<Sloth, SlothfulDbContext>(_dbContext, configurationProviderMock.Object);
        _wildKoalaService = new GetService<WildKoala, SlothfulDbContext>(_dbContext, configurationProviderMock.Object);
    }

    [Fact]
    public void Get_ShouldReturnSlothEntity()
    {
        var entityId = Guid.NewGuid();
        var entity = new Sloth(entityId, "Test", 5);

        _dbContext.Sloths.Add(entity);
        _dbContext.SaveChanges();

        var result = _slothService.Get(entity.Id);

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Name, result.Name);
        Assert.Equal(entity.Age, result.Age);
    }

    [Fact]
    public void Get_ShouldReturnWildKoalaEntity()
    {
        var cuisine = new Sloth(Guid.NewGuid(), "CuisineSloth", 3);
        var neighbour = new Sloth(Guid.NewGuid(), "NeighbourSloth", 4);
        var entityId = Guid.NewGuid();
        var entity = new WildKoala(entityId, "SpeedyKoala", 5, cuisine, neighbour);

        _dbContext.Sloths.Add(cuisine);
        _dbContext.Sloths.Add(neighbour);
        _dbContext.Koalas.Add(entity);
        _dbContext.SaveChanges();

        var result = _wildKoalaService.Get(entity.Id);

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Name, result.Name);
        Assert.Equal(entity.Age, result.Age);
        Assert.Equal(cuisine.Id, result.CuisineId);
        Assert.Equal(neighbour.Id, result.NeighbourId);
    }

    [Fact]
    public void Get_ShouldThrowException_WhenEntityNotFound()
    {
        var nonExistentId = Guid.NewGuid();

        Assert.Throws<NotFoundException>(() => _slothService.Get(nonExistentId));
        Assert.Throws<NotFoundException>(() => _wildKoalaService.Get(nonExistentId));
    }

    [Fact]
    public void Get_ShouldIncludeDependencies_ForWildKoala()
    {
        var cuisine = new Sloth(Guid.NewGuid(), "CuisineSloth", 3);
        var neighbour = new Sloth(Guid.NewGuid(), "NeighbourSloth", 4);
        var entityId = Guid.NewGuid();
        var entity = new WildKoala(entityId, "SpeedyKoala", 5, cuisine, neighbour);

        _dbContext.Sloths.Add(cuisine);
        _dbContext.Sloths.Add(neighbour);
        _dbContext.Koalas.Add(entity);
        _dbContext.SaveChanges();

        var result = _wildKoalaService.Get(entity.Id);

        Assert.NotNull(result);
        Assert.Equal(cuisine.Name, result.Cuisine.Name);
        Assert.Equal(neighbour.Name, result.Neighbour.Name);
    }
    
    [Fact]
    public void Get_ShouldThrowException_ForInvalidIdType()
    {
        var invalidId = "invalid_id";

        Assert.Throws<ConfigurationException>(() => _slothService.Get(invalidId));
    }

    [Fact]
    public void Get_ShouldThrowException_WhenConfigurationNotFound()
    {
        var nonConfiguredService = new GetService<WildKoala, SlothfulDbContext>(_dbContext, new Mock<IEntityConfigurationProvider>().Object);
        var entityId = Guid.NewGuid();

        Assert.Throws<ConfigurationException>(() => nonConfiguredService.Get(entityId));
    }

    [Fact]
    public void Get_ShouldThrowException_WhenIdIsNull()
    {
        Assert.Throws<ConfigurationException>(() => _slothService.Get(null));
    }
}
