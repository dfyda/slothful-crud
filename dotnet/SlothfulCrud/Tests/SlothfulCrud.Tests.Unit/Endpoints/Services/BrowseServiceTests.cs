using Moq;
using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Providers;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Tests.Api.EF;
using SlothfulCrud.Types.Configurations;

public class BrowseServiceTests
{
    private readonly SlothfulDbContext _dbContext;
    private readonly BrowseService<Sloth, SlothfulDbContext> _service;

    public BrowseServiceTests()
    {
        var options = new DbContextOptionsBuilder<SlothfulDbContext>()
            .UseInMemoryDatabase(databaseName: "TestBrowseDatabase")
            .Options;

        _dbContext = new SlothfulDbContext(options);
        var configurationProviderMock = new Mock<IEntityConfigurationProvider>();
        var entityConfiguration = new EntityConfiguration();
        configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
            .Returns(entityConfiguration);

        _service = new BrowseService<Sloth, SlothfulDbContext>(_dbContext, configurationProviderMock.Object);
    }

    [Fact]
    public void Browse_ShouldReturnPagedResults()
    {
        var entities = new List<Sloth>
        {
            new Sloth(Guid.NewGuid(), "Sloth1", 5),
            new Sloth(Guid.NewGuid(), "Sloth2", 6)
        };

        _dbContext.Sloths.AddRange(entities);
        _dbContext.SaveChanges();

        var query = new BrowseQuery
        {
            Rows = 10,
            Skip = 0,
            SortBy = "Name",
            SortDirection = "asc"
        };

        var result = _service.Browse(1, query);

        Assert.NotNull(result);
        Assert.Contains(entities[0], result.Data);
        Assert.Contains(entities[1], result.Data);
    }
}

public class BrowseQuery
{
    public int Rows { get; set; }
    public int Skip { get; set; }
    public string SortBy { get; set; }
    public string SortDirection { get; set; }
}
