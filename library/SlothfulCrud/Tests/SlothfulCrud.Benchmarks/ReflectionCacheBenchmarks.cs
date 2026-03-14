using System.Reflection;
using BenchmarkDotNet.Attributes;
using SlothfulCrud.Providers;
using SlothfulCrud.Tests.Api.Domain;

namespace SlothfulCrud.Benchmarks
{
    [MemoryDiagnoser]
    public class ReflectionCacheBenchmarks
    {
        private readonly Type _slothType = typeof(Sloth);
        private object _slothInstance;

        [GlobalSetup]
        public void Setup()
        {
            _slothInstance = new Sloth(Guid.NewGuid(), "BenchSloth", 5);
        }

        [Benchmark(Baseline = true)]
        public PropertyInfo[] DirectGetProperties()
        {
            return _slothType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }

        [Benchmark]
        public PropertyInfo[] CachedGetProperties()
        {
            return ReflectionCache.GetProperties(_slothType);
        }

        [Benchmark]
        public MethodInfo DirectGetMethod()
        {
            return _slothType.GetMethod("Update");
        }

        [Benchmark]
        public MethodInfo CachedGetMethod()
        {
            return ReflectionCache.GetMethod(_slothType, "Update");
        }

        [Benchmark]
        public ConstructorInfo DirectGetConstructor()
        {
            return _slothType.GetConstructors()
                .FirstOrDefault(x => x.GetParameters().Length > 0);
        }

        [Benchmark]
        public ConstructorInfo CachedGetConstructor()
        {
            return ReflectionCache.GetFirstParameterizedConstructor(_slothType);
        }

        [Benchmark]
        public IDictionary<string, object> GetPropertiesViaReflection()
        {
            var type = _slothInstance.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var dict = new Dictionary<string, object>(properties.Length);
            foreach (var prop in properties)
            {
                dict[prop.Name] = prop.GetValue(_slothInstance);
            }

            return dict;
        }

        [Benchmark]
        public IDictionary<string, object> GetPropertiesViaCachedReflection()
        {
            var properties = ReflectionCache.GetProperties(_slothInstance.GetType());
            var dict = new Dictionary<string, object>(properties.Length);
            foreach (var prop in properties)
            {
                dict[prop.Name] = prop.GetValue(_slothInstance);
            }

            return dict;
        }
    }
}