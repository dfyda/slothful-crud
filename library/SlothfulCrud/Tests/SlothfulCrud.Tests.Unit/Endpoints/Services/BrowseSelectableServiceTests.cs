using Microsoft.EntityFrameworkCore;
using Moq;
using SlothfulCrud.Providers;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Tests.Api.EF;
using SlothfulCrud.Tests.Unit.Endpoints.Queries;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Tests.Unit.Endpoints.Services
{
    public class BrowseSelectableServiceTests : IDisposable
    {
        private const string SortFieldName = "Name";
        private const string SortDirectionAsc = "asc";
        private const string SortDirectionDesc = "desc";
        private const string NonExistingPropertyName = "MissingProperty";
        private const string Sloth1Name = "Sloth1";
        private const string Sloth2Name = "Sloth2";
        private const string Sloth3Name = "Sloth3";
        private const string KoalaNeighbourAName = "KoalaNeighbourA";
        private const string KoalaNeighbourBName = "KoalaNeighbourB";
        private const string SearchSlothPrefix = "Sloth";
        private const string NeighbourSortProperty = "Neighbour";
        private const ushort FirstPage = 1;
        private const ushort SecondPage = 2;
        private const ushort ThirdPage = 3;
        private const ushort ZeroPage = 0;
        private const int OneRow = 1;
        private const int DefaultRows = 10;
        private const int PaginationRows = 5;
        private const int ZeroRows = 0;

        private readonly SlothfulDbContext _dbContext;
        private readonly BrowseSelectableService<Sloth, SlothfulDbContext> _service;

        public BrowseSelectableServiceTests()
        {
            var options = new DbContextOptionsBuilder<SlothfulDbContext>()
                .UseInMemoryDatabase(databaseName: $"BrowseSelectableServiceTests_{Guid.NewGuid()}")
                .Options;

            _dbContext = new SlothfulDbContext(options);
            var configurationProviderMock = new Mock<IEntityConfigurationProvider>();
            var entityConfiguration = new EntityConfiguration();
            configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(entityConfiguration);

            _service = new BrowseSelectableService<Sloth, SlothfulDbContext>(_dbContext,
                configurationProviderMock.Object, new SlothfulOptions());
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        private void SeedSloths(IEnumerable<Sloth> entities)
        {
            _dbContext.Sloths.AddRange(entities);
            _dbContext.SaveChanges();
        }

        private static BrowseSelectableQuery CreateQuery(
            int rows = DefaultRows,
            string sortBy = SortFieldName,
            string sortDirection = SortDirectionAsc,
            string search = null)
        {
            return new BrowseSelectableQuery
            {
                Rows = rows,
                SortBy = sortBy,
                SortDirection = sortDirection,
                Search = search
            };
        }

        [Fact]
        public async Task BrowseSelectable_ShouldReturnPagedResults_WhenEntitiesExist()
        {
            // Arrange
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), Sloth1Name, 5),
                new Sloth(Guid.NewGuid(), Sloth2Name, 6)
            };
            SeedSloths(entities);
            var query = CreateQuery();

            // Act
            var result = await _service.BrowseAsync(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DefaultRows, result.Rows);
            Assert.Contains(entities[0].Id, result.Data.Select(x => x.Id));
            Assert.Contains(entities[0].DisplayName, result.Data.Select(x => x.DisplayName));
            Assert.Contains(entities[1].Id, result.Data.Select(x => x.Id));
            Assert.Contains(entities[1].DisplayName, result.Data.Select(x => x.DisplayName));
        }

        [Fact]
        public async Task BrowseSelectable_ShouldFilterBySearchPhrase_WhenSearchProvided()
        {
            // Arrange
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), Sloth1Name, 5),
                new Sloth(Guid.NewGuid(), Sloth2Name, 6)
            };
            SeedSloths(entities);
            var query = CreateQuery(search: Sloth1Name);

            // Act
            var result = await _service.BrowseAsync(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(Sloth1Name, result.Data.First().DisplayName);
        }

        [Fact]
        public async Task BrowseSelectable_ShouldSortByNameAscending_WhenSortDirectionIsAsc()
        {
            // Arrange
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), Sloth2Name, 6),
                new Sloth(Guid.NewGuid(), Sloth1Name, 5)
            };
            SeedSloths(entities);
            var query = CreateQuery(sortDirection: SortDirectionAsc);

            // Act
            var result = await _service.BrowseAsync(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Sloth1Name, result.Data.First().DisplayName);
            Assert.Equal(Sloth2Name, result.Data.Last().DisplayName);
        }

        [Fact]
        public async Task BrowseSelectable_ShouldSortByNameDescending_WhenSortDirectionIsDesc()
        {
            // Arrange
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), Sloth1Name, 5),
                new Sloth(Guid.NewGuid(), Sloth2Name, 6)
            };
            SeedSloths(entities);
            var query = CreateQuery(sortDirection: SortDirectionDesc);

            // Act
            var result = await _service.BrowseAsync(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(Sloth2Name, result.Data.First().DisplayName);
            Assert.Equal(Sloth1Name, result.Data.Last().DisplayName);
        }

        [Fact]
        public async Task BrowseSelectable_ShouldReturnEmptyResult_WhenNoEntitiesExist()
        {
            // Arrange
            var query = CreateQuery();

            // Act
            var result = await _service.BrowseAsync(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.Total);
        }

        [Fact]
        public async Task BrowseSelectable_ShouldReturnDataWithoutSorting_WhenSortByIsNull()
        {
            // Arrange
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), Sloth2Name, 6),
                new Sloth(Guid.NewGuid(), Sloth1Name, 5)
            };
            SeedSloths(entities);
            var query = CreateQuery(sortBy: null);

            // Act
            var result = await _service.BrowseAsync(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task BrowseSelectable_ShouldSkipAndTakeCorrectNumberOfRows()
        {
            for (int i = 1; i <= 20; i++)
            {
                _dbContext.Sloths.Add(new Sloth(Guid.NewGuid(), $"Sloth{i}", i));
            }

            _dbContext.SaveChanges();

            var query = new BrowseSelectableQuery
            {
                Rows = PaginationRows,
                SortBy = SortFieldName,
                SortDirection = SortDirectionAsc
            };

            var result = await _service.BrowseAsync(ThirdPage, query);

            Assert.NotNull(result);
            Assert.Equal(PaginationRows, result.Rows);
            Assert.Equal(PaginationRows, result.Data.Count);
            Assert.Equal("Sloth19", result.Data.First().DisplayName);
        }

        [Fact]
        public async Task BrowseSelectable_ShouldReturnTotalCount()
        {
            for (int i = 1; i <= 15; i++)
            {
                _dbContext.Sloths.Add(new Sloth(Guid.NewGuid(), $"Sloth{i}", i));
            }

            _dbContext.SaveChanges();

            var query = new BrowseSelectableQuery
            {
                Rows = PaginationRows,
                SortBy = SortFieldName,
                SortDirection = SortDirectionAsc
            };

            var result = await _service.BrowseAsync(FirstPage, query);

            Assert.NotNull(result);
            Assert.Equal(15, result.Total);
        }

        [Fact]
        public async Task BrowseSelectable_ShouldReturnBaseEntityDtoWithCorrectProperties()
        {
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), Sloth1Name, 5),
                new Sloth(Guid.NewGuid(), Sloth2Name, 6)
            };

            _dbContext.Sloths.AddRange(entities);
            _dbContext.SaveChanges();

            var query = new BrowseSelectableQuery
            {
                Rows = DefaultRows,
                SortBy = SortFieldName,
                SortDirection = SortDirectionAsc
            };

            var result = await _service.BrowseAsync(FirstPage, query);

            Assert.NotNull(result);
            Assert.Contains(entities[0].Id, result.Data.Select(x => x.Id));
            Assert.Contains(entities[0].DisplayName, result.Data.Select(x => x.DisplayName));
            Assert.Contains(entities[1].Id, result.Data.Select(x => x.Id));
            Assert.Contains(entities[1].DisplayName, result.Data.Select(x => x.DisplayName));
        }

        [Fact]
        public async Task BrowseSelectable_ShouldThrowArgumentNullException_WhenQueryIsNull()
        {
            // Act + Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.BrowseAsync(FirstPage, null));
        }

        [Fact]
        public async Task BrowseSelectable_ShouldFilterAndSort_WhenSearchAndSortingAreProvided()
        {
            // Arrange
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), Sloth3Name, 5),
                new Sloth(Guid.NewGuid(), Sloth2Name, 6),
                new Sloth(Guid.NewGuid(), Sloth1Name, 5)
            };

            SeedSloths(entities);
            var query = CreateQuery(search: SearchSlothPrefix);

            // Act
            var result = await _service.BrowseAsync(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal(Sloth1Name, result.Data.First().DisplayName);
            Assert.Equal(Sloth3Name, result.Data.Last().DisplayName);
        }

        [Fact]
        public async Task BrowseSelectable_ShouldReturnSecondPage_WhenComplexQueryIsProvided()
        {
            // Arrange
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), Sloth1Name, 5),
                new Sloth(Guid.NewGuid(), Sloth2Name, 6),
                new Sloth(Guid.NewGuid(), Sloth3Name, 7)
            };

            SeedSloths(entities);
            var query = CreateQuery(rows: OneRow, search: SearchSlothPrefix);

            // Act
            var result = await _service.BrowseAsync(SecondPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(Sloth2Name, result.Data.First().DisplayName);
        }

        [Fact]
        public async Task BrowseSelectable_ShouldSortByNestedProperty_WhenSortByTargetsNavigationProperty()
        {
            // Arrange
            var cuisine = new Sloth(Guid.NewGuid(), "CuisineSloth", 3);
            var neighbourA = new Sloth(Guid.NewGuid(), "NeighbourA", 4);
            var neighbourB = new Sloth(Guid.NewGuid(), "NeighbourB", 5);

            var entities = new List<WildKoala>
            {
                new WildKoala(Guid.NewGuid(), KoalaNeighbourAName, 5, cuisine, neighbourA),
                new WildKoala(Guid.NewGuid(), KoalaNeighbourBName, 6, cuisine, neighbourB)
            };

            _dbContext.Sloths.Add(cuisine);
            _dbContext.Sloths.Add(neighbourA);
            _dbContext.Sloths.Add(neighbourB);
            _dbContext.Koalas.AddRange(entities);
            _dbContext.SaveChanges();

            var configurationProviderMock = new Mock<IEntityConfigurationProvider>();
            var entityConfiguration = new EntityConfiguration();
            entityConfiguration.SetSortProperty(NeighbourSortProperty);
            configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(WildKoala)))
                .Returns(entityConfiguration);
            configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(new EntityConfiguration());

            var service =
                new BrowseSelectableService<WildKoala, SlothfulDbContext>(_dbContext, configurationProviderMock.Object, new SlothfulOptions());

            var query = CreateQuery(sortBy: NeighbourSortProperty, sortDirection: SortDirectionDesc);

            // Act
            var result = await service.BrowseAsync(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(KoalaNeighbourBName, result.Data.First().DisplayName);
            Assert.Equal(KoalaNeighbourAName, result.Data.Last().DisplayName);
        }

        [Fact]
        public async Task BrowseSelectable_ShouldThrowInvalidOperationException_WhenFilterPropertyIsInvalid()
        {
            // Arrange
            var entityConfiguration = new EntityConfiguration();
            entityConfiguration.SetFilterProperty(NonExistingPropertyName);

            var configurationProviderMock = new Mock<IEntityConfigurationProvider>();
            configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(entityConfiguration);

            var service = new BrowseSelectableService<Sloth, SlothfulDbContext>(_dbContext, configurationProviderMock.Object, new SlothfulOptions());
            var query = CreateQuery(search: SearchSlothPrefix);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.BrowseAsync(FirstPage, query));
        }

        [Fact]
        public async Task BrowseSelectable_ShouldReturnEmptyData_WhenRowsIsZero()
        {
            // Arrange
            _dbContext.Sloths.Add(new Sloth(Guid.NewGuid(), Sloth1Name, 5));
            _dbContext.SaveChanges();

            var query = CreateQuery(rows: ZeroRows);

            // Act
            var result = await _service.BrowseAsync(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(1, result.Total);
        }

        [Fact]
        public async Task BrowseSelectable_ShouldNotSkipResults_WhenPageIsZero()
        {
            // Arrange
            _dbContext.Sloths.AddRange(
                new Sloth(Guid.NewGuid(), Sloth1Name, 5),
                new Sloth(Guid.NewGuid(), Sloth2Name, 6));
            _dbContext.SaveChanges();

            var query = CreateQuery(rows: OneRow);

            // Act
            var result = await _service.BrowseAsync(ZeroPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(Sloth1Name, result.Data.First().DisplayName);
        }

        [Fact]
        public async Task BrowseSelectable_ShouldUseConfiguredNonDefaultKeyProperty_ForBaseEntityDtoId()
        {
            // Arrange
            var entities = new List<Sloth>
            {
                new Sloth(Guid.NewGuid(), Sloth1Name, 5),
                new Sloth(Guid.NewGuid(), Sloth2Name, 6)
            };
            SeedSloths(entities);

            var configurationProviderMock = new Mock<IEntityConfigurationProvider>();
            var entityConfiguration = new EntityConfiguration();
            entityConfiguration.SetKeyProperty(nameof(Sloth.Age));
            entityConfiguration.SetKeyPropertyType(typeof(int));
            configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(entityConfiguration);

            var service = new BrowseSelectableService<Sloth, SlothfulDbContext>(_dbContext, configurationProviderMock.Object, new SlothfulOptions());
            var query = CreateQuery();

            // Act
            var result = await service.BrowseAsync(FirstPage, query);

            // Assert
            Assert.Contains(5, result.Data.Select(x => x.Id));
            Assert.Contains(6, result.Data.Select(x => x.Id));
        }

        [Fact]
        public async Task BrowseSelectable_ShouldReturnResults_WhenQueryCustomizerIsNull()
        {
            // Arrange
            SeedSloths([new Sloth(Guid.NewGuid(), Sloth1Name, 5)]);

            var configurationProviderMock = new Mock<IEntityConfigurationProvider>();
            configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(new EntityConfiguration());

            var options = new SlothfulOptions { QueryCustomizer = null };
            var service = new BrowseSelectableService<Sloth, SlothfulDbContext>(
                _dbContext, configurationProviderMock.Object, options);
            var query = CreateQuery();

            // Act
            var result = await service.BrowseAsync(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task BrowseSelectable_ShouldReturnResults_WhenQueryCustomizerIsNoOp()
        {
            // Arrange
            SeedSloths([new Sloth(Guid.NewGuid(), Sloth1Name, 5)]);

            var configurationProviderMock = new Mock<IEntityConfigurationProvider>();
            configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(new EntityConfiguration());

            var options = new SlothfulOptions { QueryCustomizer = q => q };
            var service = new BrowseSelectableService<Sloth, SlothfulDbContext>(
                _dbContext, configurationProviderMock.Object, options);
            var query = CreateQuery();

            // Act
            var result = await service.BrowseAsync(FirstPage, query);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task BrowseSelectable_ShouldThrowInvalidOperationException_WhenQueryCustomizerReturnsInvalidType()
        {
            // Arrange
            SeedSloths([new Sloth(Guid.NewGuid(), Sloth1Name, 5)]);

            var configurationProviderMock = new Mock<IEntityConfigurationProvider>();
            configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(new EntityConfiguration());

            var options = new SlothfulOptions
            {
                QueryCustomizer = q => new List<string>().AsQueryable()
            };
            var service = new BrowseSelectableService<Sloth, SlothfulDbContext>(
                _dbContext, configurationProviderMock.Object, options);
            var query = CreateQuery();

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.BrowseAsync(FirstPage, query));
        }
    }
}