---
title: Performance benchmarking
layout: home
parent: Advanced topics
nav_order: 3.3
---

# Performance benchmarking

This page describes how to run performance benchmarks locally.

## Run benchmarks

Use the benchmark project from repository root:

```bash
dotnet run -c Release --project library/SlothfulCrud/Tests/SlothfulCrud.Benchmarks -- --filter "*"
```

BenchmarkDotNet output files are generated in:

`library/SlothfulCrud/Tests/SlothfulCrud.Benchmarks/BenchmarkDotNet.Artifacts/results/`

This page focuses only on local benchmark execution and artifact location.
