using System.Reflection;
using BenchmarkDotNet.Attributes;
using SlothfulCrud.Providers;

namespace SlothfulCrud.Benchmarks
{
    [MemoryDiagnoser]
    public class QueryObjectProviderBenchmarks
    {
        private readonly Type _queryType = typeof(SampleQuery);

        [Benchmark(Baseline = true)]
        public PropertyInfo[] DirectGetQueryProperties()
        {
            return _queryType.GetProperties();
        }

        [Benchmark]
        public PropertyInfo[] CachedGetQueryProperties()
        {
            return ReflectionCache.GetProperties(_queryType);
        }

        [Benchmark]
        public MethodInfo DirectGetParseMethod()
        {
            return typeof(int).GetMethods()
                .FirstOrDefault(x => x.Name == "Parse"
                                     && x.GetParameters().Length == 1
                                     && x.GetParameters()[0].ParameterType == typeof(string));
        }

        [Benchmark]
        public MethodInfo CachedGetParseMethod()
        {
            return ReflectionCache.GetParseMethod(typeof(int));
        }

        public class SampleQuery
        {
            public string Name { get; set; }
            public int? Age { get; set; }
            public DateTime? CreatedAtFrom { get; set; }
            public DateTime? CreatedAtTo { get; set; }
            public int Rows { get; set; }
            public string SortBy { get; set; }
            public string SortDirection { get; set; }
        }
    }
}