---
title: SlothfulOptions
layout: home
parent: Configurations
grand_parent: Fundamentals
nav_order: 2.2.2
---

# SlothfulOptions

`SlothfulOptions` is the global configuration object for Slothful CRUD. It is registered as a singleton in the DI container when you call `AddSlothfulCrud<TDbContext, TAssemblyMarker>()`.

You can configure options via the `configureOptions` callback on both `AddSlothfulCrud` and `UseSlothfulCrud`.

### Available Properties

- **UseSlothfulProblemHandling** (`bool`, default `false`): Enables the built-in exception middleware that returns standardized `problem+json` responses. See [Problem handling](../problem-handling.html) for details.
- **QueryCustomizer** (`Func<IQueryable, IQueryable>`, default `null`): Optional hook to customize every EF Core query before execution. Useful for applying provider-specific optimizations such as `AsSplitQuery()`.

### Example Usage

```csharp
builder.Services.AddSlothfulCrud<SlothfulDbContext, Program>(options =>
{
    options.UseSlothfulProblemHandling = true;
});

app.UseSlothfulCrud<SlothfulDbContext, Program>();
```

### Using QueryCustomizer

The `QueryCustomizer` hook receives a non-generic `IQueryable` and must return an `IQueryable` of the same element type. It is applied to every read query (`Get`, `Browse`, `BrowseSelectable`) before execution.

```csharp
using Microsoft.EntityFrameworkCore;

builder.Services.AddSlothfulCrud<SlothfulDbContext, Program>(options =>
{
    options.QueryCustomizer = query => query is IQueryable<object> q
        ? q.AsSplitQuery()
        : query;
});
```

{: .important }
`AsSplitQuery()` requires the `Microsoft.EntityFrameworkCore.Relational` package and a relational database provider. It has no effect with the InMemory provider.

### Migration from Legacy API

If you are using the deprecated single-generic overloads:

```csharp
// Before (deprecated)
builder.Services.AddSlothfulCrud<SlothfulDbContext>();
app.UseSlothfulCrud<SlothfulDbContext>(options => options.UseSlothfulProblemHandling = true);

// After (recommended)
builder.Services.AddSlothfulCrud<SlothfulDbContext, Program>(options =>
{
    options.UseSlothfulProblemHandling = true;
});
app.UseSlothfulCrud<SlothfulDbContext, Program>();
```

The `TAssemblyMarker` type parameter can be any type from the assembly containing your entity classes (e.g., `Program`, or one of your entity types). This ensures reliable assembly discovery in all hosting scenarios, including unit tests with `WebApplicationFactory`.
