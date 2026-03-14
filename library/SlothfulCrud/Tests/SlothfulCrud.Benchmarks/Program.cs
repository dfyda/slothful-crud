using BenchmarkDotNet.Running;
using SlothfulCrud.Benchmarks;

var switcher = BenchmarkSwitcher.FromAssembly(typeof(ReflectionCacheBenchmarks).Assembly);
switcher.Run(args);
