using Moq;
using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Providers;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Tests.Api.EF;
using SlothfulCrud.Types.Configurations;

public class BrowseSelectableServiceTests : IDisposable
{
    private readonly SlothfulDbContext _dbContext;
    private readonly BrowseSelectableService<Sloth, SlothfulDbContext> _service;

    public BrowseSelectableServiceTests()
    {
        var options = new DbContextOptionsBuilder<SlothfulDbContext>()
            .UseInMemoryDatabase(databaseName: "TestBrowseSelectableDatabase")
            .Options;

        _dbContext = new SlothfulDbContext(options);
        var configurationProviderMock = new Mock<IEntityConfigurationProvider>();
        var entityConfiguration = new EntityConfiguration();
        configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
            .Returns(entityConfiguration);

        _service = new BrowseSelectableService<Sloth, SlothfulDbContext>(_dbContext, configurationProviderMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Fact]
    public void BrowseSelectable_ShouldReturnPagedResults()
    {
        var entities = new List<Sloth>
        {
            new Sloth(Guid.NewGuid(), "Sloth1", 5),
            new Sloth(Guid.NewGuid(), "Sloth2", 6)
        };

        _dbContext.Sloths.AddRange(entities);
        _dbContext.SaveChanges();

        var query = new BrowseSelectableQuery
        {
            Rows = 10,
            SortBy = "Name",
            SortDirection = "asc"
        };

        var result = _service.Browse(1, query);

        Assert.NotNull(result);
        Assert.Contains(entities[0].Id, result.Data.Select(x => x.Id));
        Assert.Contains(entities[0].DisplayName, result.Data.Select(x => x.DisplayName));
        Assert.Contains(entities[1].Id, result.Data.Select(x => x.Id));
        Assert.Contains(entities[1].DisplayName, result.Data.Select(x => x.DisplayName));
    }

    [Fact]
    public void BrowseSelectable_ShouldFilterByStringField()
    {
        var entities = new List<Sloth>
        {
            new Sloth(Guid.NewGuid(), "Sloth1", 5),
            new Sloth(Guid.NewGuid(), "Sloth2", 6)
        };

        _dbContext.Sloths.AddRange(entities);
        _dbContext.SaveChanges();

        var query = new BrowseSelectableQuery
        {
            Rows = 10,
            SortBy = "Name",
            SortDirection = "asc",
            Search = "Sloth1"
        };

        var result = _service.Browse(1, query);

        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal("Sloth1", result.Data.First().DisplayName);
    }

    [Fact]
    public void BrowseSelectable_ShouldSortByStringFieldAscending()
    {
        var entities = new List<Sloth>
        {
            new Sloth(Guid.NewGuid(), "Sloth2", 6),
            new Sloth(Guid.NewGuid(), "Sloth1", 5)
        };

        _dbContext.Sloths.AddRange(entities);
        _dbContext.SaveChanges();

        var query = new BrowseSelectableQuery
        {
            Rows = 10,
            SortBy = "Name",
            SortDirection = "asc"
        };

        var result = _service.Browse(1, query);

        Assert.NotNull(result);
        Assert.Equal("Sloth1", result.Data.First().DisplayName);
        Assert.Equal("Sloth2", result.Data.Last().DisplayName);
    }

    [Fact]
    public void BrowseSelectable_ShouldSortByStringFieldDescending()
    {
        var entities = new List<Sloth>
        {
            new Sloth(Guid.NewGuid(), "Sloth1", 5),
            new Sloth(Guid.NewGuid(), "Sloth2", 6)
        };

        _dbContext.Sloths.AddRange(entities);
        _dbContext.SaveChanges();

        var query = new BrowseSelectableQuery
        {
            Rows = 10,
            SortBy = "Name",
            SortDirection = "desc"
        };

        var result = _service.Browse(1, query);

        Assert.NotNull(result);
        Assert.Equal("Sloth2", result.Data.First().DisplayName);
        Assert.Equal("Sloth1", result.Data.Last().DisplayName);
    }

    [Fact]
    public void BrowseSelectable_ShouldHandleEmptyResult()
    {
        var query = new BrowseSelectableQuery
        {
            Rows = 10,
            SortBy = "Name",
            SortDirection = "asc"
        };

        var result = _service.Browse(1, query);

        Assert.NotNull(result);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public void BrowseSelectable_ShouldHandleNullSortByField()
    {
        var entities = new List<Sloth>
        {
            new Sloth(Guid.NewGuid(), "Sloth2", 6),
            new Sloth(Guid.NewGuid(), "Sloth1", 5)
        };

        _dbContext.Sloths.AddRange(entities);
        _dbContext.SaveChanges();

        var query = new BrowseSelectableQuery
        {
            Rows = 10,
            SortBy = null,
            SortDirection = "asc"
        };

        var result = _service.Browse(1, query);

        Assert.NotNull(result);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public void BrowseSelectable_ShouldSkipAndTakeCorrectNumberOfRows()
    {
        for (int i = 1; i <= 20; i++)
        {
            _dbContext.Sloths.Add(new Sloth(Guid.NewGuid(), $"Sloth{i}", i));
        }
        _dbContext.SaveChanges();

        var query = new BrowseSelectableQuery
        {
            Rows = 5,
            SortBy = "Name",
            SortDirection = "asc"
        };

        var result = _service.Browse(3, query);

        Assert.NotNull(result);
        Assert.Equal(5, result.Data.Count);
        Assert.Equal("Sloth19", result.Data.First().DisplayName);
    }

    [Fact]
    public void BrowseSelectable_ShouldReturnTotalCount()
    {
        for (int i = 1; i <= 15; i++)
        {
            _dbContext.Sloths.Add(new Sloth(Guid.NewGuid(), $"Sloth{i}", i));
        }
        _dbContext.SaveChanges();

        var query = new BrowseSelectableQuery
        {
            Rows = 5,
            SortBy = "Name",
            SortDirection = "asc"
        };

        var result = _service.Browse(1, query);

        Assert.NotNull(result);
        Assert.Equal(15, result.Total);
    }

    // TODO: Something is wrong with filtering by date, it returns the same result
    [Fact]
    public void BrowseSelectable_ShouldFilterByDateField()
    {
        var cuisine = new Sloth(Guid.NewGuid(), "CuisineSloth", 3);
        var neighbour1 = new Sloth(Guid.NewGuid(), "NeighbourSloth1", 4);
        var neighbour2 = new Sloth(Guid.NewGuid(), "NeighbourSloth2", 5);

        var entities = new List<WildKoala>
        {
            new WildKoala(Guid.NewGuid(), "Koala1", 5, cuisine, neighbour1),
            new WildKoala(Guid.NewGuid(), "Koala2", 6, cuisine, neighbour2)
        };

        typeof(WildKoala).GetProperty("CreatedAt").SetValue(entities[0], new DateTime(2023, 1, 1));
        typeof(WildKoala).GetProperty("CreatedAt").SetValue(entities[1], new DateTime(2023, 2, 1));

        _dbContext.Sloths.Add(cuisine);
        _dbContext.Sloths.Add(neighbour1);
        _dbContext.Sloths.Add(neighbour2);
        _dbContext.Koalas.AddRange(entities);
        _dbContext.SaveChanges();

        var configurationProviderMock = new Mock<IEntityConfigurationProvider>();
        var entityConfiguration = new EntityConfiguration();
        configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(WildKoala)))
            .Returns(entityConfiguration);

        var service = new BrowseSelectableService<WildKoala, SlothfulDbContext>(_dbContext, configurationProviderMock.Object);

        var query = new BrowseSelectableQuery
        {
            Rows = 10,
            SortBy = "Name",
            SortDirection = "asc",
            CreatedAtFrom = new DateTime(2023, 1, 15)
        };

        var result = service.Browse(1, query);

        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal("Koala2", result.Data.First().DisplayName);
    }

    [Fact]
    public void BrowseSelectable_ShouldReturnBaseEntityDtoWithCorrectProperties()
    {
        var entities = new List<Sloth>
        {
            new Sloth(Guid.NewGuid(), "Sloth1", 5),
            new Sloth(Guid.NewGuid(), "Sloth2", 6)
        };

        _dbContext.Sloths.AddRange(entities);
        _dbContext.SaveChanges();

        var query = new BrowseSelectableQuery
        {
            Rows = 10,
            SortBy = "Name",
            SortDirection = "asc"
        };

        var result = _service.Browse(1, query);

        Assert.NotNull(result);
        Assert.Contains(entities[0].Id, result.Data.Select(x => x.Id));
        Assert.Contains(entities[0].DisplayName, result.Data.Select(x => x.DisplayName));
        Assert.Contains(entities[1].Id, result.Data.Select(x => x.Id));
        Assert.Contains(entities[1].DisplayName, result.Data.Select(x => x.DisplayName));
    }

    [Fact]
    public void BrowseSelectable_ShouldHandleNullQuery()
    {
        Assert.Throws<ArgumentNullException>(() => _service.Browse(1, null));
    }

    [Fact]
    public void BrowseSelectable_ShouldFilterAndSortByMultipleFields()
    {
        var entities = new List<Sloth>
        {
            new Sloth(Guid.NewGuid(), "Sloth3", 5),
            new Sloth(Guid.NewGuid(), "Sloth2", 6),
            new Sloth(Guid.NewGuid(), "Sloth1", 5)
        };

        _dbContext.Sloths.AddRange(entities);
        _dbContext.SaveChanges();

        var query = new BrowseSelectableQuery
        {
            Rows = 10,
            SortBy = "Name",
            SortDirection = "asc",
            Search = "Sloth"
        };

        var result = _service.Browse(1, query);

        Assert.NotNull(result);
        Assert.Equal(3, result.Data.Count);
        Assert.Equal("Sloth1", result.Data.First().DisplayName);
        Assert.Equal("Sloth3", result.Data.Last().DisplayName);
    }

    [Fact]
    public void BrowseSelectable_ShouldHandleComplexQueries()
    {
        var entities = new List<Sloth>
        {
            new Sloth(Guid.NewGuid(), "Sloth1", 5),
            new Sloth(Guid.NewGuid(), "Sloth2", 6),
            new Sloth(Guid.NewGuid(), "Sloth3", 7)
        };

        _dbContext.Sloths.AddRange(entities);
        _dbContext.SaveChanges();

        var query = new BrowseSelectableQuery
        {
            Rows = 1,
            SortBy = "Name",
            SortDirection = "asc",
            Search = "Sloth"
        };

        var result = _service.Browse(2, query);

        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal("Sloth2", result.Data.First().DisplayName);
    }

    // TODO: Something is wrong with sorting by nested property, asc and desc return the same result
    [Fact]
    public void BrowseSelectable_ShouldSortByNestedProperty()
    {
        var cuisine = new Sloth(Guid.NewGuid(), "CuisineSloth", 3);
        var neighbour1 = new Sloth(Guid.NewGuid(), "NeighbourSloth1", 4);
        var neighbour2 = new Sloth(Guid.NewGuid(), "NeighbourSloth2", 5);

        var entities = new List<WildKoala>
        {
            new WildKoala(Guid.NewGuid(), "Koala1", 5, cuisine, neighbour1),
            new WildKoala(Guid.NewGuid(), "Koala2", 6, cuisine, neighbour2)
        };

        _dbContext.Sloths.Add(cuisine);
        _dbContext.Sloths.Add(neighbour1);
        _dbContext.Sloths.Add(neighbour2);
        _dbContext.Koalas.AddRange(entities);
        _dbContext.SaveChanges();

        var configurationProviderMock = new Mock<IEntityConfigurationProvider>();
        var entityConfiguration = new EntityConfiguration();
        entityConfiguration.SetSortProperty("Neighbour");
        configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(WildKoala)))
            .Returns(entityConfiguration);

        var service = new BrowseSelectableService<WildKoala, SlothfulDbContext>(_dbContext, configurationProviderMock.Object);

        var query = new BrowseSelectableQuery
        {
            Rows = 10,
            SortBy = "Neighbour",
            SortDirection = "asc"
        };

        var result = service.Browse(1, query);

        Assert.NotNull(result);
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(entities[0].DisplayName, result.Data.First().DisplayName);
        Assert.Equal(entities[1].DisplayName, result.Data.Last().DisplayName);
    }
}

public class BrowseSelectableQuery
{
    public int Rows { get; set; }
    public string SortBy { get; set; }
    public string SortDirection { get; set; }
    public string Search { get; set; }
    public DateTime? CreatedAtFrom { get; set; }
}
