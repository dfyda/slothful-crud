using Microsoft.Extensions.DependencyInjection;
using Moq;
using SlothfulCrud.Services;
using SlothfulCrud.Services.Endpoints.Delete;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Services.Endpoints.Post;
using SlothfulCrud.Services.Endpoints.Put;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Tests.Api.EF;

namespace SlothfulCrud.Tests.Unit.Endpoints.Services
{
    public class EndpointsServiceTests
    {
        private readonly Mock<IGetService<Sloth, SlothfulDbContext>> _getServiceMock;
        private readonly Mock<IDeleteService<Sloth, SlothfulDbContext>> _deleteServiceMock;
        private readonly Mock<ICreateService<Sloth, SlothfulDbContext>> _createServiceMock;
        private readonly Mock<IUpdateService<Sloth, SlothfulDbContext>> _updateServiceMock;
        private readonly Mock<IBrowseService<Sloth, SlothfulDbContext>> _browseServiceMock;
        private readonly Mock<IBrowseSelectableService<Sloth, SlothfulDbContext>> _browseSelectableServiceMock;
        private readonly EndpointsService<Sloth, SlothfulDbContext> _service;

        public EndpointsServiceTests()
        {
            _getServiceMock = new Mock<IGetService<Sloth, SlothfulDbContext>>();
            _deleteServiceMock = new Mock<IDeleteService<Sloth, SlothfulDbContext>>();
            _createServiceMock = new Mock<ICreateService<Sloth, SlothfulDbContext>>();
            _updateServiceMock = new Mock<IUpdateService<Sloth, SlothfulDbContext>>();
            _browseServiceMock = new Mock<IBrowseService<Sloth, SlothfulDbContext>>();
            _browseSelectableServiceMock = new Mock<IBrowseSelectableService<Sloth, SlothfulDbContext>>();

            _service = new EndpointsService<Sloth, SlothfulDbContext>(
                _getServiceMock.Object,
                _deleteServiceMock.Object,
                _createServiceMock.Object,
                _updateServiceMock.Object,
                _browseServiceMock.Object,
                _browseSelectableServiceMock.Object
            );
        }

        [Fact]
        public void Get_ShouldCallGetService()
        {
            var id = Guid.NewGuid();
            _service.Get(id);
            _getServiceMock.Verify(s => s.Get(id), Times.Once);
        }

        [Fact]
        public void Delete_ShouldCallDeleteService()
        {
            var id = Guid.NewGuid();
            _service.Delete(id);
            _deleteServiceMock.Verify(s => s.Delete(id), Times.Once);
        }

        [Fact]
        public void Create_ShouldCallCreateService()
        {
            var id = Guid.NewGuid();
            var command = new { Name = "Test", Age = 5 };
            var serviceScopeMock = new Mock<IServiceScope>();

            _service.Create(id, command, serviceScopeMock.Object);
            _createServiceMock.Verify(s => s.Create(id, command, serviceScopeMock.Object), Times.Once);
        }

        [Fact]
        public void Update_ShouldCallUpdateService()
        {
            var id = Guid.NewGuid();
            var command = new { Name = "Test", Age = 5 };
            var serviceScopeMock = new Mock<IServiceScope>();

            _service.Update(id, command, serviceScopeMock.Object);
            _updateServiceMock.Verify(s => s.Update(id, command, serviceScopeMock.Object), Times.Once);
        }

        [Fact]
        public void Browse_ShouldCallBrowseService()
        {
            ushort page = 1;
            var query = new { Search = "Test" };

            _service.Browse(page, query);
            _browseServiceMock.Verify(s => s.Browse(page, query), Times.Once);
        }

        [Fact]
        public void BrowseSelectable_ShouldCallBrowseSelectableService()
        {
            ushort page = 1;
            var query = new { Search = "Test" };

            _service.BrowseSelectable(page, query);
            _browseSelectableServiceMock.Verify(s => s.Browse(page, query), Times.Once);
        }
    }
}