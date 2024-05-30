using Microsoft.EntityFrameworkCore;
using Moq;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Providers;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Tests.Api.EF;
using SlothfulCrud.Tests.Unit.Endpoints.Queries;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Tests.Unit.Endpoints.Services
{
    public class BrowseServiceTests : IDisposable
    {
        private SlothfulDbContext _dbContext;
        private Mock<IEntityConfigurationProvider> _configurationProviderMock;

        public BrowseServiceTests()
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
                .UseInMemoryDatabase(databaseName: "TestBrowseDatabase")
                .Options;

            _dbContext = new SlothfulDbContext(options);
        }
        
        private void ConfigureMocks()
        {
            _configurationProviderMock = new Mock<IEntityConfigurationProvider>();
            var entityConfiguration = new EntityConfiguration();
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(entityConfiguration);
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(WildKoala)))
                .Returns(entityConfiguration);
        }
        
        private BrowseService<Sloth, SlothfulDbContext> GetSlothService()
        {
            return new BrowseService<Sloth, SlothfulDbContext>(
                _dbContext,
                _configurationProviderMock.Object
            );
        }
        
        private BrowseService<WildKoala, SlothfulDbContext> GetWildKoalaService()
        {
            return new BrowseService<WildKoala, SlothfulDbContext>(
                _dbContext,
                _configurationProviderMock.Object
            );
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
                SortBy = "Name",
                SortDirection = "asc"
            };

            var result = GetSlothService().Browse(1, query);

            Assert.NotNull(result);
            Assert.Contains(entities[0], result.Data);
            Assert.Contains(entities[1], result.Data);
        }

        [Fact]
        public void Browse_ShouldFilterByStringField()
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
                SortBy = "Name",
                SortDirection = "asc",
                Name = "Sloth1"
            };

            var result = GetSlothService().Browse(1, query);

            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("Sloth1", result.Data.First().Name);
        }

        [Fact]
        public void Browse_ShouldSortByStringFieldAscending()
        {
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), "Sloth2", 6),
                new Sloth(Guid.NewGuid(), "Sloth1", 5)
            };

            _dbContext.Sloths.AddRange(entities);
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = 10,
                SortBy = "Name",
                SortDirection = "asc"
            };

            var result = GetSlothService().Browse(1, query);

            Assert.NotNull(result);
            Assert.Equal("Sloth1", result.Data.First().Name);
            Assert.Equal("Sloth2", result.Data.Last().Name);
        }

        [Fact]
        public void Browse_ShouldSortByStringFieldDescending()
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
                SortBy = "Name",
                SortDirection = "desc"
            };

            var result = GetSlothService().Browse(1, query);

            Assert.NotNull(result);
            Assert.Equal("Sloth2", result.Data.First().Name);
            Assert.Equal("Sloth1", result.Data.Last().Name);
        }

        [Fact]
        public void Browse_ShouldThrowException_WhenSortFieldNotFound()
        {
            var query = new BrowseQuery
            {
                Rows = 10,
                SortBy = "InvalidField",
                SortDirection = "asc"
            };

            Assert.Throws<ConfigurationException>(() => GetSlothService().Browse(1, query));
        }

        [Fact]
        public void Browse_ShouldHandleNullSortByField()
        {
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), "Sloth2", 6),
                new Sloth(Guid.NewGuid(), "Sloth1", 5)
            };

            _dbContext.Sloths.AddRange(entities);
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = 10,
                SortBy = null,
                SortDirection = "asc"
            };

            var result = GetSlothService().Browse(1, query);

            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public void Browse_ShouldSkipAndTakeCorrectNumberOfRows()
        {
            for (int i = 1; i <= 9; i++)
            {
                _dbContext.Sloths.Add(new Sloth(Guid.NewGuid(), $"Sloth{i}", i));
            }

            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = 2,
                SortBy = "Name",
                SortDirection = "asc"
            };

            var result = GetSlothService().Browse(5, query);

            Assert.NotNull(result);
            Assert.Equal(1, result.Data.Count);
            Assert.Equal("Sloth9", result.Data.First().Name);
        }

        [Fact]
        public void Browse_ShouldReturnTotalCount()
        {
            for (int i = 1; i <= 15; i++)
            {
                _dbContext.Sloths.Add(new Sloth(Guid.NewGuid(), $"Sloth{i}", i));
            }

            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = 5,
                SortBy = "Name",
                SortDirection = "asc"
            };

            var result = GetSlothService().Browse(1, query);

            Assert.NotNull(result);
            Assert.Equal(15, result.Total);
        }

        [Fact]
        public void Browse_ShouldHandleEmptyResult()
        {
            var query = new BrowseQuery
            {
                Rows = 10,
                SortBy = "Name",
                SortDirection = "asc"
            };

            var result = GetSlothService().Browse(1, query);

            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.Total);
        }

        [Fact]
        public void Browse_ShouldFilterByDateField()
        {
            var cuisine1 = new Sloth(Guid.NewGuid(), "Cuisine1", 3);
            var cuisine2 = new Sloth(Guid.NewGuid(), "Cuisine2", 3);

            var entities = new List<WildKoala>
            {
                new WildKoala(Guid.NewGuid(), "Koala1", 5, cuisine1, null),
                new WildKoala(Guid.NewGuid(), "Koala2", 6, cuisine2, null)
            };

            typeof(WildKoala).GetProperty("CreatedAt").SetValue(entities[0], new DateTime(2023, 1, 1));
            typeof(WildKoala).GetProperty("CreatedAt").SetValue(entities[1], new DateTime(2023, 2, 1));

            _dbContext.Koalas.AddRange(entities);
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = 10,
                SortBy = "Name",
                SortDirection = "asc",
                CreatedAtFrom = new DateTime(2023, 1, 15)
            };

            var result = GetWildKoalaService().Browse(1, query);

            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("Koala2", result.Data.First().Name);
        }

        [Fact]
        public void Browse_ShouldSortWildKoalaByStringFieldAscending()
        {
            var cuisine1 = new Sloth(Guid.NewGuid(), "Cuisine1", 3);
            var cuisine2 = new Sloth(Guid.NewGuid(), "Cuisine2", 3);

            var entities = new List<WildKoala>
            {
                new WildKoala(Guid.NewGuid(), "Koala2", 6, cuisine1, null),
                new WildKoala(Guid.NewGuid(), "Koala1", 5, cuisine2, null)
            };

            _dbContext.Koalas.AddRange(entities);
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = 10,
                SortBy = "Name",
                SortDirection = "asc"
            };

            var result = GetWildKoalaService().Browse(1, query);

            Assert.NotNull(result);
            Assert.Equal("Koala1", result.Data.First().Name);
            Assert.Equal("Koala2", result.Data.Last().Name);
        }

        [Fact]
        public void Browse_ShouldFilterWildKoalaByStringField()
        {
            var cuisine1 = new Sloth(Guid.NewGuid(), "Cuisine1", 3);
            var cuisine2 = new Sloth(Guid.NewGuid(), "Cuisine2", 3);

            var entities = new List<WildKoala>
            {
                new WildKoala(Guid.NewGuid(), "Koala1", 5, cuisine1, null),
                new WildKoala(Guid.NewGuid(), "Koala2", 6, cuisine2, null)
            };

            _dbContext.Koalas.AddRange(entities);
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = 10,
                SortBy = "Name",
                SortDirection = "asc",
                Name = "Koala1"
            };

            var result = GetWildKoalaService().Browse(1, query);

            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("Koala1", result.Data.First().Name);
        }

        [Fact]
        public void Browse_ShouldSortWildKoalaByStringFieldDescending()
        {
            var cuisine1 = new Sloth(Guid.NewGuid(), "Cuisine1", 3);
            var cuisine2 = new Sloth(Guid.NewGuid(), "Cuisine2", 3);

            var entities = new List<WildKoala>
            {
                new WildKoala(Guid.NewGuid(), "Koala1", 5, cuisine1, null),
                new WildKoala(Guid.NewGuid(), "Koala2", 6, cuisine2, null)
            };

            _dbContext.Koalas.AddRange(entities);
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = 10,
                SortBy = "Name",
                SortDirection = "desc"
            };

            var result = GetWildKoalaService().Browse(1, query);

            Assert.NotNull(result);
            Assert.Equal("Koala2", result.Data.First().Name);
            Assert.Equal("Koala1", result.Data.Last().Name);
        }

        [Fact]
        public void Browse_ShouldThrowException_WhenInvalidSortDirection()
        {
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), "Sloth2", 6),
                new Sloth(Guid.NewGuid(), "Sloth1", 5)
            };

            _dbContext.Sloths.AddRange(entities);
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = 10,
                SortBy = "Name",
                SortDirection = "invalid"
            };

            Assert.Throws<ConfigurationException>(() => GetSlothService().Browse(1, query));
        }

        [Fact]
        public void Browse_ShouldThrowException_WhenQueryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => GetSlothService().Browse(1, null));
        }

        [Fact]
        public void Browse_ShouldThrowException_WhenSortPropertyIsNotFound()
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
                SortBy = "InvalidProperty",
                SortDirection = "asc"
            };

            Assert.Throws<ConfigurationException>(() => GetSlothService().Browse(1, query));
        }
    }
}