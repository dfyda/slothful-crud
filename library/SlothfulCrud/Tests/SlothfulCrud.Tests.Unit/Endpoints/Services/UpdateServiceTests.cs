using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Providers;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Services.Endpoints.Put;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Tests.Api.EF;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Tests.Unit.Endpoints.Services
{
    public class UpdateServiceTests
    {
        private SlothfulDbContext _dbContext;
        private Mock<IGetService<Sloth, SlothfulDbContext>> _slothGetServiceMock;
        private Mock<IGetService<WildKoala, SlothfulDbContext>> _wildKoalaGetServiceMock;
        private Mock<IEntityConfigurationProvider> _configurationProviderMock;
        private Mock<IServiceScope> _serviceScopeMock;
        private Mock<IValidator<Sloth>> _slothValidatorMock;
        private Mock<IValidator<WildKoala>> _wildKoalaValidatorMock;

        public UpdateServiceTests()
        {
            ConfigureDbContext();
            ConfigureMocks();
        }
        
        private void ConfigureDbContext()
        {
            var options = new DbContextOptionsBuilder<SlothfulDbContext>()
                .UseInMemoryDatabase(databaseName: "TestUpdateDatabase")
                .Options;

            _dbContext = new SlothfulDbContext(options);
        }

        private void ConfigureMocks()
        {
            _slothGetServiceMock = new Mock<IGetService<Sloth, SlothfulDbContext>>();
            _wildKoalaGetServiceMock = new Mock<IGetService<WildKoala, SlothfulDbContext>>();
            _configurationProviderMock = new Mock<IEntityConfigurationProvider>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _slothValidatorMock = new Mock<IValidator<Sloth>>();
            _wildKoalaValidatorMock = new Mock<IValidator<WildKoala>>();
            
            var wildKoalaEntityConfiguration = new EntityConfiguration();
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(WildKoala)))
                .Returns(wildKoalaEntityConfiguration);
        }

        private void ConfigureAdditionalMocks()
        {
            var configuration = new EntityConfiguration();
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(configuration);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IValidator<Sloth>)))
                .Returns(_slothValidatorMock.Object);

            _serviceScopeMock.Setup(scope => scope.ServiceProvider)
                .Returns(serviceProviderMock.Object);
            
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(WildKoala)))
                .Returns(configuration);
            
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IValidator<WildKoala>)))
                .Returns(_wildKoalaValidatorMock.Object);

            _serviceScopeMock.Setup(scope => scope.ServiceProvider)
                .Returns(serviceProviderMock.Object);
        }
        
        private UpdateService<Sloth, SlothfulDbContext> GetSlothService()
        {
            return new UpdateService<Sloth, SlothfulDbContext>(
                _dbContext,
                _slothGetServiceMock.Object,
                _configurationProviderMock.Object
            );
        }
        
        private UpdateService<WildKoala, SlothfulDbContext> GetWildKoalaService()
        {
            return new UpdateService<WildKoala, SlothfulDbContext>(
                _dbContext,
                _wildKoalaGetServiceMock.Object,
                _configurationProviderMock.Object
            );
        }

        [Fact]
        public void Update_ShouldInvokeModifyMethod()
        {
            var entityId = Guid.NewGuid();
            var entity = new Sloth(entityId, "OldSloth", 5);
            var command = new { name = "UpdatedSloth", age = 10 };

            ConfigureAdditionalMocks();

            _dbContext.Sloths.Add(entity);
            _dbContext.SaveChanges();

            _slothGetServiceMock.Setup(s => s.Get(entityId)).Returns(entity);

            GetSlothService().Update(entityId, command, _serviceScopeMock.Object);

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

            ConfigureAdditionalMocks();

            _slothValidatorMock.Setup(v => v.Validate(It.IsAny<IValidationContext>())).Returns(new ValidationResult());

            _dbContext.Sloths.Add(entity);
            _dbContext.SaveChanges();

            _slothGetServiceMock.Setup(s => s.Get(entityId)).Returns(entity);

            GetSlothService().Update(entityId, command, _serviceScopeMock.Object);

            _slothValidatorMock.Verify(v => v.Validate(It.IsAny<IValidationContext>()), Times.Once);
        }

        [Fact]
        public void Update_WildKoala_ShouldUpdateEntityInDbContext()
        {
            var entityId = Guid.NewGuid();
            var cuisine = new Sloth(Guid.NewGuid(), "CuisineSloth", 3);
            var neighbour = new Sloth(Guid.NewGuid(), "NeighbourSloth", 4);
            var entity = new WildKoala(entityId, "OldKoala", 5, cuisine, neighbour);
            var cuisineId = cuisine.Id;
            var neighbourId = neighbour.Id;
            var command = new { name = "UpdatedKoala", age = 10, age2 = (int?)null, cuisineId, neighbourId };

            ConfigureAdditionalMocks();

            _dbContext.Sloths.Add(cuisine);
            _dbContext.Sloths.Add(neighbour);
            _dbContext.Koalas.Add(entity);
            _dbContext.SaveChanges();

            _wildKoalaGetServiceMock.Setup(s => s.Get(entityId)).Returns(entity);

            GetWildKoalaService().Update(entityId, command, _serviceScopeMock.Object);

            var updatedEntity = _dbContext.Koalas.Find(entityId);
            Assert.NotNull(updatedEntity);
            Assert.Equal("UpdatedKoala", updatedEntity.Name);
            Assert.Equal(10, updatedEntity.Age);
            Assert.Null(updatedEntity.Age2);
            Assert.Equal(cuisine.Id, updatedEntity.CuisineId);
            Assert.Equal(neighbour.Id, updatedEntity.NeighbourId);
        }

        [Fact]
        public void Update_WildKoala_ShouldValidateEntity_WhenValidationEnabled()
        {
            var entityId = Guid.NewGuid();
            var cuisine = new Sloth(Guid.NewGuid(), "CuisineSloth", 3);
            var neighbour = new Sloth(Guid.NewGuid(), "NeighbourSloth", 4);
            var entity = new WildKoala(entityId, "OldKoala", 5, cuisine, neighbour);
            var cuisineId = cuisine.Id;
            var neighbourId = neighbour.Id;
            var command = new { name = "UpdatedKoala", age = 10, age2 = (int?)null, cuisineId, neighbourId };

            ConfigureAdditionalMocks();

            _dbContext.Sloths.Add(cuisine);
            _dbContext.Sloths.Add(neighbour);
            _dbContext.Koalas.Add(entity);
            _dbContext.SaveChanges();

            _wildKoalaGetServiceMock.Setup(s => s.Get(entityId)).Returns(entity);

            GetWildKoalaService().Update(entityId, command, _serviceScopeMock.Object);

            _wildKoalaValidatorMock.Verify(v => v.Validate(It.IsAny<IValidationContext>()), Times.Once);
        }

        [Fact]
        public void Update_ShouldThrowException_ForInvalidIdType()
        {
            var invalidId = "invalid_id";
            var command = new { name = "UpdatedSloth", age = 10 };

            var configuration = new EntityConfiguration();
            configuration.SetUpdateMethod("Update");
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(configuration);

            Assert.Throws<ConfigurationException>(() =>
                GetSlothService().Update(invalidId, command, _serviceScopeMock.Object));
        }

        [Fact]
        public void Update_ShouldThrowException_WhenConfigurationNotFound()
        {
            var nonConfiguredService = new UpdateService<Sloth, SlothfulDbContext>(_dbContext,
                _slothGetServiceMock.Object, new Mock<IEntityConfigurationProvider>().Object);
            var entityId = Guid.NewGuid();
            var command = new { name = "UpdatedSloth", age = 10 };

            Assert.Throws<ConfigurationException>(() =>
                nonConfiguredService.Update(entityId, command, _serviceScopeMock.Object));
        }

        [Fact]
        public void Update_ShouldThrowException_WhenIdIsNull()
        {
            var command = new { name = "UpdatedSloth", age = 10 };

            var configuration = new EntityConfiguration();
            configuration.SetUpdateMethod("Update");
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(configuration);

            Assert.Throws<ConfigurationException>(() => GetSlothService().Update(null, command, _serviceScopeMock.Object));
        }

        [Fact]
        public void Update_ShouldThrowException_WhenUpdateMethodNotFound()
        {
            var entityId = Guid.NewGuid();
            var entity = new Sloth(entityId, "OldSloth", 5);
            var command = new { name = "UpdatedSloth", age = 10 };

            var configuration = new EntityConfiguration();
            configuration.SetUpdateMethod("NotExistingMethod");
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(configuration);

            _dbContext.Sloths.Add(entity);
            _dbContext.SaveChanges();

            _slothGetServiceMock.Setup(s => s.Get(entityId)).Returns(entity);

            Assert.Throws<ConfigurationException>(() =>
                GetSlothService().Update(entityId, command, _serviceScopeMock.Object));
        }
    }
}