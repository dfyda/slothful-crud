﻿using Microsoft.EntityFrameworkCore;
using Moq;
using SlothfulCrud.Exceptions;
using SlothfulCrud.Providers;
using SlothfulCrud.Services.Endpoints.Delete;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Tests.Api.EF;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Tests.Unit.Endpoints.Services
{
    public class DeleteServiceTests
    {
        private SlothfulDbContext _dbContext;
        private Mock<IGetService<Sloth, SlothfulDbContext>> _getServiceMock;
        private Mock<IEntityConfigurationProvider> _configurationProviderMock;

        public DeleteServiceTests()
        {
            ConfigureDbContext();
            ConfigureMocks();
        }

        private void ConfigureDbContext()
        {
            var options = new DbContextOptionsBuilder<SlothfulDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dbContext = new SlothfulDbContext(options);
        }
        
        private void ConfigureMocks()
        {
            _getServiceMock = new Mock<IGetService<Sloth, SlothfulDbContext>>();
            
            _configurationProviderMock = new Mock<IEntityConfigurationProvider>();
            var slothEntityConfiguration = new EntityConfiguration();
            _configurationProviderMock.Setup(cp => cp.GetConfiguration(typeof(Sloth)))
                .Returns(slothEntityConfiguration);
        }

        private DeleteService<Sloth, SlothfulDbContext> GetService()
        {
            return new DeleteService<Sloth, SlothfulDbContext>(_dbContext, _getServiceMock.Object,
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

            GetService().Delete(entityId);

            var deletedEntity = _dbContext.Sloths.Find(entityId);
            Assert.Null(deletedEntity);
        }

        [Fact]
        public void Delete_ShouldThrowException_WhenEntityNotFound()
        {
            var entityId = Guid.NewGuid();
            _getServiceMock.Setup(s => s.Get(entityId)).Throws(new NotFoundException());

            Assert.Throws<NotFoundException>(() => GetService().Delete(entityId));
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

            GetService().Delete(entityId);

            _getServiceMock.Verify(s => s.Get(entityId), Times.Once);
        }

        [Fact]
        public void Delete_ShouldThrowException_WhenKeyPropertyIsNull()
        {
            _getServiceMock.Setup(s => s.Get(null)).Throws(new ConfigurationException());

            Assert.Throws<ConfigurationException>(() => GetService().Delete(null));
        }
    }
}