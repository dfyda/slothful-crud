using BenchmarkDotNet.Attributes;
using SlothfulCrud.Tests.Api.Domain;
using SlothfulCrud.Types;

namespace SlothfulCrud.Benchmarks
{
    [MemoryDiagnoser]
    public class DynamicTypeMappingBenchmarks
    {
        private Type _dtoType;
        private Sloth _slothEntity;
        private List<Sloth> _slothEntities;
        private PagedResults<Sloth> _pagedResults;

        [GlobalSetup]
        public void Setup()
        {
            _dtoType = DynamicType.NewDynamicTypeDto(typeof(Sloth), "SlothBenchDto", false);

            _slothEntity = new Sloth(Guid.NewGuid(), "BenchmarkSloth", 5);

            _slothEntities = Enumerable.Range(0, 100)
                .Select(i => new Sloth(Guid.NewGuid(), $"Sloth{i}", i))
                .ToList();

            _pagedResults = new PagedResults<Sloth>(0, 100, 100, _slothEntities);
        }

        [Benchmark]
        public dynamic MapSingleEntity()
        {
            return DynamicType.MapToDto(_slothEntity, typeof(Sloth), _dtoType, false, "Id");
        }

        [Benchmark]
        public dynamic MapPagedResults_100Items()
        {
            return DynamicType.MapToPagedResultsDto(_pagedResults, typeof(Sloth), _dtoType, false, "Id");
        }

        /// <summary>
        /// Measures the cache-hit path — the real production scenario where the type
        /// already exists in the dynamic assembly. Creating a unique type per iteration
        /// causes O(n²) degradation because AssemblyBuilder.GetType() scans all emitted
        /// types and Reflection.Emit types are never collected.
        /// </summary>
        [Benchmark]
        public Type CreateDtoType_CacheHit()
        {
            return DynamicType.NewDynamicTypeDto(typeof(Sloth), "SlothBenchDto_Cached", false);
        }
    }
}