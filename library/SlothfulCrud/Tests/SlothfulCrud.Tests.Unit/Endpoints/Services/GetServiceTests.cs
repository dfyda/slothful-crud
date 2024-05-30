using Microsoft.EntityFrameworkCore;
using Moq;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Providers;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Tests.Api.EF;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Tests.Unit.Endpoints.Services
{
    public class GetServiceTests
    {
        private SlothfulDbContext _dbContext;
        private Mock<IEntityConfigurationProvider> _configurationProviderMock;

        public GetServiceTests()
        {
            ConfigureDbContext();
            ConfigureMocks();
        }
        
        private void ConfigureDbContext()
        {
            var options = new DbContextOptionsBuilder<SlothfulDbContext>()
                .UseInMemoryDatabase(databaseName: "TestGetDatabase")
                .Options;

            _dbContext = new SlothfulDbContext(options);
        }
        
        private void ConfigureMocks()
        {
            _configurationProviderMock = new Mock<IEntityConfigurationProvider>();
            
            var slothEntityConfiguration = new EntityConfiguration();
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(slothEntityConfiguration);

            var wildKoalaEntityConfiguration = new EntityConfiguration();
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(WildKoala)))
                .Returns(wildKoalaEntityConfiguration);
        }
        
        private GetService<Sloth, SlothfulDbContext> GetSlothService()
        {
            return new GetService<Sloth, SlothfulDbContext>(_dbContext, _configurationProviderMock.Object);
        }
        
        private GetService<WildKoala, SlothfulDbContext> GetWildKoalaService()
        {
            return 
                new GetService<WildKoala, SlothfulDbContext>(_dbContext, _configurationProviderMock.Object);
        }

        [Fact]
        public void Get_ShouldReturnSlothEntity()
        {
            var entityId = Guid.NewGuid();
            var entity = new Sloth(entityId, "Test", 5);

            _dbContext.Sloths.Add(entity);
            _dbContext.SaveChanges();

            var result = GetSlothService().Get(entity.Id);

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

            var result = GetWildKoalaService().Get(entity.Id);

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

            Assert.Throws<NotFoundException>(() => GetSlothService().Get(nonExistentId));
            Assert.Throws<NotFoundException>(() => GetWildKoalaService().Get(nonExistentId));
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

            var result = GetWildKoalaService().Get(entity.Id);

            Assert.NotNull(result);
            Assert.Equal(cuisine.Name, result.Cuisine.Name);
            Assert.Equal(neighbour.Name, result.Neighbour.Name);
        }

        [Fact]
        public void Get_ShouldThrowException_ForInvalidIdType()
        {
            var invalidId = "invalid_id";

            Assert.Throws<ConfigurationException>(() => GetSlothService().Get(invalidId));
        }

        [Fact]
        public void Get_ShouldThrowException_WhenConfigurationNotFound()
        {
            var nonConfiguredService =
                new GetService<WildKoala, SlothfulDbContext>(_dbContext,
                    new Mock<IEntityConfigurationProvider>().Object);
            var entityId = Guid.NewGuid();

            Assert.Throws<ConfigurationException>(() => nonConfiguredService.Get(entityId));
        }

        [Fact]
        public void Get_ShouldThrowException_WhenIdIsNull()
        {
            Assert.Throws<ConfigurationException>(() => GetSlothService().Get(null));
        }
    }
}