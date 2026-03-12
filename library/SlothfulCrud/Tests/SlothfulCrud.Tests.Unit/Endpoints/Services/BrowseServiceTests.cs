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
        private const string SortFieldName = "Name";
        private const string SortDirectionAsc = "asc";
        private const string SortDirectionDesc = "desc";
        private const string CreatedAtPropertyName = "CreatedAt";
        private const string NonExistingPropertyName = "InvalidProperty";
        private const string InvalidSortFieldName = "InvalidField";
        private const string InvalidSortDirection = "invalid";
        private const string Sloth1Name = "Sloth1";
        private const string Sloth2Name = "Sloth2";
        private const string Koala1Name = "Koala1";
        private const string Koala2Name = "Koala2";
        private const string Cuisine1Name = "Cuisine1";
        private const string Cuisine2Name = "Cuisine2";
        private const string CuisineName = "Cuisine";
        private const ushort FirstPage = 1;
        private const ushort ZeroPage = 0;
        private const ushort FifthPage = 5;
        private const int SingleRow = 1;
        private const int TwoRows = 2;
        private const int DefaultRows = 10;
        private const int PaginationRows = 5;
        private const int ZeroRows = 0;
        private static readonly DateTime CreatedAtJan1 = new(2023, 1, 1);
        private static readonly DateTime CreatedAtJan2 = new(2023, 1, 2);
        private static readonly DateTime CreatedAtJan10 = new(2023, 1, 10);
        private static readonly DateTime CreatedAtJan15 = new(2023, 1, 15);
        private static readonly DateTime CreatedAtJan31 = new(2023, 1, 31);
        private static readonly DateTime CreatedAtFeb1 = new(2023, 2, 1);
        private static readonly DateTime CreatedAtFeb10 = new(2023, 2, 10);

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
                .UseInMemoryDatabase(databaseName: $"BrowseServiceTests_{Guid.NewGuid()}")
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

        private static BrowseQuery CreateQuery(
            int rows = DefaultRows,
            string sortBy = SortFieldName,
            string sortDirection = SortDirectionAsc)
        {
            return new BrowseQuery
            {
                Rows = rows,
                SortBy = sortBy,
                SortDirection = sortDirection
            };
        }

        [Fact]
        public void Browse_ShouldReturnPagedResults_WhenEntitiesExist()
        {
            // Arrange
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), Sloth1Name, 5),
                new Sloth(Guid.NewGuid(), Sloth2Name, 6)
            };

            _dbContext.Sloths.AddRange(entities);
            _dbContext.SaveChanges();

            var query = CreateQuery();

            // Act
            var result = GetSlothService().Browse(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DefaultRows, result.Rows);
            Assert.Contains(entities[0], result.Data);
            Assert.Contains(entities[1], result.Data);
        }

        [Fact]
        public void Browse_ShouldFilterByStringField_WhenFilterValueProvided()
        {
            // Arrange
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), Sloth1Name, 5),
                new Sloth(Guid.NewGuid(), Sloth2Name, 6)
            };

            _dbContext.Sloths.AddRange(entities);
            _dbContext.SaveChanges();

            var query = CreateQuery();
            query.Name = Sloth1Name;

            // Act
            var result = GetSlothService().Browse(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(Sloth1Name, result.Data.First().Name);
        }

        [Fact]
        public void Browse_ShouldSortByNameAscending_WhenSortDirectionIsAsc()
        {
            // Arrange
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), Sloth2Name, 6),
                new Sloth(Guid.NewGuid(), Sloth1Name, 5)
            };

            _dbContext.Sloths.AddRange(entities);
            _dbContext.SaveChanges();

            var query = CreateQuery();

            // Act
            var result = GetSlothService().Browse(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Sloth1Name, result.Data.First().Name);
            Assert.Equal(Sloth2Name, result.Data.Last().Name);
        }

        [Fact]
        public void Browse_ShouldSortByNameDescending_WhenSortDirectionIsDesc()
        {
            // Arrange
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), Sloth1Name, 5),
                new Sloth(Guid.NewGuid(), Sloth2Name, 6)
            };

            _dbContext.Sloths.AddRange(entities);
            _dbContext.SaveChanges();

            var query = CreateQuery(sortDirection: SortDirectionDesc);

            // Act
            var result = GetSlothService().Browse(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Sloth2Name, result.Data.First().Name);
            Assert.Equal(Sloth1Name, result.Data.Last().Name);
        }

        [Fact]
        public void Browse_ShouldThrowConfigurationException_WhenSortFieldNotFound()
        {
            // Arrange
            var query = new BrowseQuery
            {
                Rows = DefaultRows,
                SortBy = InvalidSortFieldName,
                SortDirection = SortDirectionAsc
            };

            // Act + Assert
            Assert.Throws<ConfigurationException>(() => GetSlothService().Browse(FirstPage, query));
        }

        [Fact]
        public void Browse_ShouldReturnDataWithoutSorting_WhenSortByIsNull()
        {
            // Arrange
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), Sloth2Name, 6),
                new Sloth(Guid.NewGuid(), Sloth1Name, 5)
            };

            _dbContext.Sloths.AddRange(entities);
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = DefaultRows,
                SortBy = null,
                SortDirection = SortDirectionAsc
            };

            // Act
            var result = GetSlothService().Browse(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public void Browse_ShouldReturnPageSlice_WhenPaginationParametersAreProvided()
        {
            // Arrange
            for (int i = 1; i <= 9; i++)
            {
                _dbContext.Sloths.Add(new Sloth(Guid.NewGuid(), $"Sloth{i}", i));
            }

            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = 2,
                SortBy = SortFieldName,
                SortDirection = SortDirectionAsc
            };

            // Act
            var result = GetSlothService().Browse(FifthPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Rows);
            Assert.Single(result.Data);
            Assert.Equal("Sloth9", result.Data.First().Name);
        }

        [Fact]
        public void Browse_ShouldReturnTotalCount_WhenEntitiesExist()
        {
            // Arrange
            for (int i = 1; i <= 15; i++)
            {
                _dbContext.Sloths.Add(new Sloth(Guid.NewGuid(), $"Sloth{i}", i));
            }

            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = PaginationRows,
                SortBy = SortFieldName,
                SortDirection = SortDirectionAsc
            };

            // Act
            var result = GetSlothService().Browse(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(15, result.Total);
        }

        [Fact]
        public void Browse_ShouldReturnEmptyResult_WhenNoEntitiesExist()
        {
            // Arrange
            var query = new BrowseQuery
            {
                Rows = DefaultRows,
                SortBy = SortFieldName,
                SortDirection = SortDirectionAsc
            };

            // Act
            var result = GetSlothService().Browse(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.Total);
        }

        [Fact]
        public void Browse_ShouldFilterByCreatedAtFrom_WhenDateLowerBoundProvided()
        {
            // Arrange
            var cuisine1 = new Sloth(Guid.NewGuid(), Cuisine1Name, 3);
            var cuisine2 = new Sloth(Guid.NewGuid(), Cuisine2Name, 3);

            var entities = new List<WildKoala>
            {
                new WildKoala(Guid.NewGuid(), Koala1Name, 5, cuisine1, null),
                new WildKoala(Guid.NewGuid(), Koala2Name, 6, cuisine2, null)
            };

            typeof(WildKoala).GetProperty(CreatedAtPropertyName).SetValue(entities[0], CreatedAtJan1);
            typeof(WildKoala).GetProperty(CreatedAtPropertyName).SetValue(entities[1], CreatedAtFeb1);

            _dbContext.Koalas.AddRange(entities);
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = DefaultRows,
                SortBy = SortFieldName,
                SortDirection = SortDirectionAsc,
                CreatedAtFrom = CreatedAtJan15
            };

            // Act
            var result = GetWildKoalaService().Browse(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(Koala2Name, result.Data.First().Name);
        }

        [Fact]
        public void Browse_ShouldSortWildKoalaByNameAscending_WhenSortDirectionIsAsc()
        {
            // Arrange
            var cuisine1 = new Sloth(Guid.NewGuid(), Cuisine1Name, 3);
            var cuisine2 = new Sloth(Guid.NewGuid(), Cuisine2Name, 3);

            var entities = new List<WildKoala>
            {
                new WildKoala(Guid.NewGuid(), Koala2Name, 6, cuisine1, null),
                new WildKoala(Guid.NewGuid(), Koala1Name, 5, cuisine2, null)
            };

            _dbContext.Koalas.AddRange(entities);
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = DefaultRows,
                SortBy = SortFieldName,
                SortDirection = SortDirectionAsc
            };

            // Act
            var result = GetWildKoalaService().Browse(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Koala1Name, result.Data.First().Name);
            Assert.Equal(Koala2Name, result.Data.Last().Name);
        }

        [Fact]
        public void Browse_ShouldFilterWildKoalaByName_WhenFilterValueProvided()
        {
            // Arrange
            var cuisine1 = new Sloth(Guid.NewGuid(), Cuisine1Name, 3);
            var cuisine2 = new Sloth(Guid.NewGuid(), Cuisine2Name, 3);

            var entities = new List<WildKoala>
            {
                new WildKoala(Guid.NewGuid(), Koala1Name, 5, cuisine1, null),
                new WildKoala(Guid.NewGuid(), Koala2Name, 6, cuisine2, null)
            };

            _dbContext.Koalas.AddRange(entities);
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = DefaultRows,
                SortBy = SortFieldName,
                SortDirection = SortDirectionAsc,
                Name = Koala1Name
            };

            // Act
            var result = GetWildKoalaService().Browse(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(Koala1Name, result.Data.First().Name);
        }

        [Fact]
        public void Browse_ShouldSortWildKoalaByNameDescending_WhenSortDirectionIsDesc()
        {
            // Arrange
            var cuisine1 = new Sloth(Guid.NewGuid(), Cuisine1Name, 3);
            var cuisine2 = new Sloth(Guid.NewGuid(), Cuisine2Name, 3);

            var entities = new List<WildKoala>
            {
                new WildKoala(Guid.NewGuid(), Koala1Name, 5, cuisine1, null),
                new WildKoala(Guid.NewGuid(), Koala2Name, 6, cuisine2, null)
            };

            _dbContext.Koalas.AddRange(entities);
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = DefaultRows,
                SortBy = SortFieldName,
                SortDirection = SortDirectionDesc
            };

            // Act
            var result = GetWildKoalaService().Browse(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Koala2Name, result.Data.First().Name);
            Assert.Equal(Koala1Name, result.Data.Last().Name);
        }

        [Fact]
        public void Browse_ShouldThrowConfigurationException_WhenSortDirectionIsInvalid()
        {
            // Arrange
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), Sloth2Name, 6),
                new Sloth(Guid.NewGuid(), Sloth1Name, 5)
            };

            _dbContext.Sloths.AddRange(entities);
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = DefaultRows,
                SortBy = SortFieldName,
                SortDirection = InvalidSortDirection
            };

            // Act + Assert
            Assert.Throws<ConfigurationException>(() => GetSlothService().Browse(FirstPage, query));
        }

        [Fact]
        public void Browse_ShouldThrowArgumentNullException_WhenQueryIsNull()
        {
            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => GetSlothService().Browse(FirstPage, null));
        }

        [Fact]
        public void Browse_ShouldThrowConfigurationException_WhenSortPropertyIsNotFound()
        {
            // Arrange
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), Sloth1Name, 5),
                new Sloth(Guid.NewGuid(), Sloth2Name, 6)
            };

            _dbContext.Sloths.AddRange(entities);
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = DefaultRows,
                SortBy = NonExistingPropertyName,
                SortDirection = SortDirectionAsc
            };

            // Act + Assert
            Assert.Throws<ConfigurationException>(() => GetSlothService().Browse(FirstPage, query));
        }

        [Fact]
        public void Browse_ShouldFilterByCreatedAtTo_WhenDateUpperBoundProvided()
        {
            // Arrange
            var cuisine = new Sloth(Guid.NewGuid(), CuisineName, 3);
            var entities = new List<WildKoala>
            {
                new WildKoala(Guid.NewGuid(), Koala1Name, 5, cuisine, null),
                new WildKoala(Guid.NewGuid(), Koala2Name, 6, cuisine, null)
            };

            typeof(WildKoala).GetProperty(CreatedAtPropertyName)!.SetValue(entities[0], CreatedAtJan1);
            typeof(WildKoala).GetProperty(CreatedAtPropertyName)!.SetValue(entities[1], CreatedAtFeb1);

            _dbContext.Koalas.AddRange(entities);
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = DefaultRows,
                SortBy = SortFieldName,
                SortDirection = SortDirectionAsc,
                CreatedAtTo = CreatedAtJan10
            };

            // Act
            var result = GetWildKoalaService().Browse(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(Koala1Name, result.Data.First().Name);
        }

        [Fact]
        public void Browse_ShouldFilterByCreatedAtRange_WhenBothBoundsProvided()
        {
            // Arrange
            var cuisine = new Sloth(Guid.NewGuid(), CuisineName, 3);
            var entities = new List<WildKoala>
            {
                new WildKoala(Guid.NewGuid(), Koala1Name, 5, cuisine, null),
                new WildKoala(Guid.NewGuid(), Koala2Name, 6, cuisine, null),
                new WildKoala(Guid.NewGuid(), "Koala3", 7, cuisine, null)
            };

            typeof(WildKoala).GetProperty(CreatedAtPropertyName)!.SetValue(entities[0], CreatedAtJan1);
            typeof(WildKoala).GetProperty(CreatedAtPropertyName)!.SetValue(entities[1], CreatedAtJan15);
            typeof(WildKoala).GetProperty(CreatedAtPropertyName)!.SetValue(entities[2], CreatedAtFeb10);

            _dbContext.Koalas.AddRange(entities);
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = DefaultRows,
                SortBy = SortFieldName,
                SortDirection = SortDirectionAsc,
                CreatedAtFrom = CreatedAtJan2,
                CreatedAtTo = CreatedAtJan31
            };

            // Act
            var result = GetWildKoalaService().Browse(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(Koala2Name, result.Data.First().Name);
        }

        [Fact]
        public void Browse_ShouldReturnEmptyData_WhenRowsIsZero()
        {
            // Arrange
            _dbContext.Sloths.Add(new Sloth(Guid.NewGuid(), Sloth1Name, 5));
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = ZeroRows,
                SortBy = SortFieldName,
                SortDirection = SortDirectionAsc
            };

            // Act
            var result = GetSlothService().Browse(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(1, result.Total);
        }

        [Fact]
        public void Browse_ShouldNotSkipResults_WhenPageIsZero()
        {
            // Arrange
            _dbContext.Sloths.AddRange(
                new Sloth(Guid.NewGuid(), Sloth1Name, 5),
                new Sloth(Guid.NewGuid(), Sloth2Name, 6));
            _dbContext.SaveChanges();

            var query = new BrowseQuery
            {
                Rows = SingleRow,
                SortBy = SortFieldName,
                SortDirection = SortDirectionAsc
            };

            // Act
            var result = GetSlothService().Browse(ZeroPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(Sloth1Name, result.Data.First().Name);
        }
    }
}