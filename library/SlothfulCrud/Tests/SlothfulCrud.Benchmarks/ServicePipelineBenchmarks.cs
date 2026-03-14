using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using SlothfulCrud.Providers;
using SlothfulCrud.Services.Endpoints.Get;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Tests.Api.EF;
using SlothfulCrud.Types.Configurations;

namespace SlothfulCrud.Benchmarks
{
    [MemoryDiagnoser]
    public class ServicePipelineBenchmarks
    {
        private SlothfulDbContext _dbContext;
        private GetService<Sloth, SlothfulDbContext> _getService;
        private BrowseService<Sloth, SlothfulDbContext> _browseService;
        private BrowseSelectableService<Sloth, SlothfulDbContext> _browseSelectableService;
        private Guid _knownSlothId;

        [GlobalSetup]
        public void Setup()
        {
            var dbOptions = new DbContextOptionsBuilder<SlothfulDbContext>()
                .UseInMemoryDatabase($"PipelineBenchmarks_{Guid.NewGuid()}")
                .Options;

            _dbContext = new SlothfulDbContext(dbOptions);

            for (int i = 0; i < 100; i++)
            {
                _dbContext.Sloths.Add(new Sloth(Guid.NewGuid(), $"Sloth{i}", i));
            }

            _dbContext.SaveChanges();
            _knownSlothId = _dbContext.Sloths.First().Id;

            var configProvider = new EntityConfigurationProvider();
            var slothfulOptions = new SlothfulOptions();

            _getService = new GetService<Sloth, SlothfulDbContext>(_dbContext, configProvider, slothfulOptions);
            _browseService = new BrowseService<Sloth, SlothfulDbContext>(_dbContext, configProvider, slothfulOptions);
            _browseSelectableService = new BrowseSelectableService<Sloth, SlothfulDbContext>(_dbContext, configProvider, slothfulOptions);
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Benchmark]
        public async Task<Sloth> GetAsync()
        {
            return await _getService.GetAsync(_knownSlothId);
        }

        [Benchmark]
        public async Task BrowseAsync_NoFilters()
        {
            var query = new BrowseQueryBench { Rows = 10, SortBy = "Name", SortDirection = "asc" };
            await _browseService.BrowseAsync(1, query);
        }

        [Benchmark]
        public async Task BrowseAsync_WithNameFilter()
        {
            var query = new BrowseQueryBench { Rows = 10, SortBy = "Name", SortDirection = "asc", Name = "Sloth5" };
            await _browseService.BrowseAsync(1, query);
        }

        [Benchmark]
        public async Task BrowseSelectableAsync()
        {
            var query = new BrowseSelectableQueryBench { Rows = 10, SortBy = "Name", SortDirection = "asc" };
            await _browseSelectableService.BrowseAsync(1, query);
        }

        public class BrowseQueryBench
        {
            public int Rows { get; set; }
            public string SortBy { get; set; }
            public string SortDirection { get; set; }
            public string Name { get; set; }
        }

        public class BrowseSelectableQueryBench
        {
            public int Rows { get; set; }
            public string SortBy { get; set; }
            public string SortDirection { get; set; }
            public string Search { get; set; }
        }
    }
}
