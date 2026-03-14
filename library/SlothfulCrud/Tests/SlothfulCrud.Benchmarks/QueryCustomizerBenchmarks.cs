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
    public class QueryCustomizerBenchmarks
    {
        private SlothfulDbContext _dbContext;
        private Guid _knownSlothId;
        private EntityConfigurationProvider _configProvider;

        [GlobalSetup]
        public void Setup()
        {
            var dbOptions = new DbContextOptionsBuilder<SlothfulDbContext>()
                .UseInMemoryDatabase($"CustomizerBenchmarks_{Guid.NewGuid()}")
                .Options;

            _dbContext = new SlothfulDbContext(dbOptions);

            for (int i = 0; i < 100; i++)
            {
                _dbContext.Sloths.Add(new Sloth(Guid.NewGuid(), $"Sloth{i}", i));
            }

            _dbContext.SaveChanges();
            _knownSlothId = _dbContext.Sloths.First().Id;
            _configProvider = new EntityConfigurationProvider();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Benchmark(Baseline = true)]
        public async Task<Sloth> GetAsync_NoCustomizer()
        {
            var options = new SlothfulOptions { QueryCustomizer = null };
            var service = new GetService<Sloth, SlothfulDbContext>(_dbContext, _configProvider, options);
            return await service.GetAsync(_knownSlothId);
        }

        [Benchmark]
        public async Task<Sloth> GetAsync_NoOpCustomizer()
        {
            var options = new SlothfulOptions { QueryCustomizer = q => q };
            var service = new GetService<Sloth, SlothfulDbContext>(_dbContext, _configProvider, options);
            return await service.GetAsync(_knownSlothId);
        }

        [Benchmark]
        public async Task BrowseAsync_NoCustomizer()
        {
            var options = new SlothfulOptions { QueryCustomizer = null };
            var service = new BrowseService<Sloth, SlothfulDbContext>(_dbContext, _configProvider, options);
            var query = new BrowseQueryBench { Rows = 10, SortBy = "Name", SortDirection = "asc" };
            await service.BrowseAsync(1, query);
        }

        [Benchmark]
        public async Task BrowseAsync_NoOpCustomizer()
        {
            var options = new SlothfulOptions { QueryCustomizer = q => q };
            var service = new BrowseService<Sloth, SlothfulDbContext>(_dbContext, _configProvider, options);
            var query = new BrowseQueryBench { Rows = 10, SortBy = "Name", SortDirection = "asc" };
            await service.BrowseAsync(1, query);
        }

        public class BrowseQueryBench
        {
            public int Rows { get; set; }
            public string SortBy { get; set; }
            public string SortDirection { get; set; }
            public string Name { get; set; }
        }
    }
}
