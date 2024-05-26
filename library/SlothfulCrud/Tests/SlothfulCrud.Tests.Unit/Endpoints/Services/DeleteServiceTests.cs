using Microsoft.EntityFrameworkCore;
using Moq;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Providers;
using SlothfulCrud.Services.Endpoints.Delete;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Tests.Api.EF;

namespace SlothfulCrud.Tests.Unit.Endpoints.Services
{
    public class DeleteServiceTests
    {
        private readonly SlothfulDbContext _dbContext;
        private readonly DeleteService<Sloth, SlothfulDbContext> _service;
        private readonly Mock<IGetService<Sloth, SlothfulDbContext>> _getServiceMock;
        private readonly Mock<IEntityConfigurationProvider> _configurationProviderMock;

        public DeleteServiceTests()
        {
            var options = new DbContextOptionsBuilder<SlothfulDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dbContext = new SlothfulDbContext(options);
            _getServiceMock = new Mock<IGetService<Sloth, SlothfulDbContext>>();
            _configurationProviderMock = new Mock<IEntityConfigurationProvider>();
            _service = new DeleteService<Sloth, SlothfulDbContext>(_dbContext, _getServiceMock.Object,
                _configurationProviderMock.Object);
        }

        [Fact]
        public void Delete_ShouldRemoveEntityFromDbContext()
        {
            var entityId = Guid.NewGuid();
            var entity = new Sloth(entityId, "Test", 5);

            _dbContext.Sloths.RemoveRange(_dbContext.Sloths);
            _dbContext.SaveChanges();

            _dbContext.Sloths.Add(entity);
            _dbContext.SaveChanges();

            _getServiceMock.Setup(s => s.Get(entityId)).Returns(entity);

            _service.Delete(entityId);

            var deletedEntity = _dbContext.Sloths.Find(entityId);
            Assert.Null(deletedEntity);
        }

        [Fact]
        public void Delete_ShouldThrowException_WhenEntityNotFound()
        {
            var entityId = Guid.NewGuid();
            _getServiceMock.Setup(s => s.Get(entityId)).Throws(new NotFoundException());

            Assert.Throws<NotFoundException>(() => _service.Delete(entityId));
        }

        [Fact]
        public void Delete_ShouldCallGetService()
        {
            var entityId = Guid.NewGuid();
            var entity = new Sloth(entityId, "Test", 5);

            _dbContext.Sloths.RemoveRange(_dbContext.Sloths);
            _dbContext.SaveChanges();

            _dbContext.Sloths.Add(entity);
            _dbContext.SaveChanges();

            _getServiceMock.Setup(s => s.Get(entityId)).Returns(entity);

            _service.Delete(entityId);

            _getServiceMock.Verify(s => s.Get(entityId), Times.Once);
        }

        [Fact]
        public void Delete_ShouldThrowException_WhenKeyPropertyIsNull()
        {
            _getServiceMock.Setup(s => s.Get(null)).Throws(new ConfigurationException());

            Assert.Throws<ConfigurationException>(() => _service.Delete(null));
        }
    }
}