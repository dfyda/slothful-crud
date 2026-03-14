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
        private const ushort FirstPage = 1;
        private const string TestName = "Test";
        private const string SearchTerm = "Test";
        private const int TestAge = 5;

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

        private static IServiceScope CreateServiceScope()
        {
            return new Mock<IServiceScope>().Object;
        }

        [Fact]
        public async Task Get_ShouldDelegateCall_ToGetService()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            await _service.GetAsync(id);

            // Assert
            _getServiceMock.Verify(s => s.GetAsync(id), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldDelegateCall_ToDeleteService()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            await _service.DeleteAsync(id);

            // Assert
            _deleteServiceMock.Verify(s => s.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task Create_ShouldDelegateCall_ToCreateService()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new { Name = TestName, Age = TestAge };
            var scope = CreateServiceScope();

            // Act
            await _service.CreateAsync(id, command, scope);

            // Assert
            _createServiceMock.Verify(s => s.CreateAsync(id, command, scope), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldDelegateCall_ToUpdateService()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new { Name = TestName, Age = TestAge };
            var scope = CreateServiceScope();

            // Act
            await _service.UpdateAsync(id, command, scope);

            // Assert
            _updateServiceMock.Verify(s => s.UpdateAsync(id, command, scope), Times.Once);
        }

        [Fact]
        public async Task Browse_ShouldDelegateCall_ToBrowseService()
        {
            // Arrange
            ushort page = FirstPage;
            var query = new { Search = SearchTerm };

            // Act
            await _service.BrowseAsync(page, query);

            // Assert
            _browseServiceMock.Verify(s => s.BrowseAsync(page, query), Times.Once);
        }

        [Fact]
        public async Task BrowseSelectable_ShouldDelegateCall_ToBrowseSelectableService()
        {
            // Arrange
            ushort page = FirstPage;
            var query = new { Search = SearchTerm };

            // Act
            await _service.BrowseSelectableAsync(page, query);

            // Assert
            _browseSelectableServiceMock.Verify(s => s.BrowseAsync(page, query), Times.Once);
        }
    }
}
