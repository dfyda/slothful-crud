<div align="center">
    <img src="docs/assets/slothful-api.jpg" alt="slothful-api logo">
</div>

# Slothful CRUD

Slothful CRUD helps you generate CRUD API endpoints for your entities with minimal setup.  
Implement `ISlothfulEntity`, register the library, and expose REST endpoints backed by your `DbContext`.

## Documentation

Full documentation is available at [slothful.dev](https://slothful.dev/).

## Quick Start

1. Install package:

```bash
dotnet add package slothful-crud
```

2. Register services and endpoint generation:

```csharp
using SlothfulCrud.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SlothfulDbContext>(options =>
    options.UseInMemoryDatabase("AppDb"));
builder.Services.AddSlothfulCrud<SlothfulDbContext, Program>();

var app = builder.Build();
app.UseSlothfulCrud<SlothfulDbContext, Program>();

app.Run();
```

The `Program` type parameter serves as an assembly marker for entity discovery. You can use any type from the assembly containing your entities.

3. Implement `ISlothfulEntity` on your domain type:

```csharp
using SlothfulCrud.Domain;

public class Sloth : ISlothfulEntity
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public int Age { get; private set; }
    public string DisplayName => Name;

    public Sloth(Guid id, string name, int age)
    {
        Id = id;
        Name = name;
        Age = age;
    }

    public void Update(string name, int age)
    {
        Name = name;
        Age = age;
    }
}
```

## Generated Endpoints

By default, the following endpoints are generated per entity:

- `GET /{segment}/{id}` - details
- `GET /{segment}/list/{page}` - browse
- `GET /{segment}/selectable-list/{page}` - browse selectable
- `POST /{segment}` - create
- `PUT /{segment}/{id}` - update
- `DELETE /{segment}/{id}` - delete

## Important Notes

- `POST create` request body does not include `id`; the key is generated server-side.
- `201 Created` includes `Location: /{segment}/{id}` and uses `IApiSegmentProvider`.
- Validation is enabled by default (`HasValidation = true`). Register validators, e.g.:

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<YourValidator>();
```

## Common Customizations

- Endpoint segment strategy (`IApiSegmentProvider`)
- Key generation strategy (`IEntityPropertyKeyValueProvider<TEntity>`)
- Create constructor selection (`ICreateConstructorBehavior`)
- Entity and endpoint configuration (`ISlothEntityConfiguration<T>`)
- Problem handling (`UseSlothfulProblemHandling`)
- Query customization (`QueryCustomizer` in `SlothfulOptions`)

## Performance Benchmarking

BenchmarkDotNet benchmarks are included to track performance regressions and compare internal changes.

Run all benchmarks:

```bash
dotnet run -c Release --project library/SlothfulCrud/Tests/SlothfulCrud.Benchmarks -- --filter "*"
```

Results are exported to:

`library/SlothfulCrud/Tests/SlothfulCrud.Benchmarks/BenchmarkDotNet.Artifacts/results/`

See details in docs:

- [Quick start](https://slothful.dev/)
- [Endpoints](https://slothful.dev/fundamentals/endpoints.html)
- [Entity configuration](https://slothful.dev/fundamentals/configurations/entity-configuration.html)
- [Validators](https://slothful.dev/fundamentals/validators.html)
- [Problem handling](https://slothful.dev/fundamentals/problem-handling.html)
- [Advanced topics](https://slothful.dev/advanced-topics.html)

## Package and License

- NuGet: [slothful-crud](https://www.nuget.org/packages/slothful-crud)
- License: [MIT](https://github.com/dfyda/slothful-crud/blob/main/LICENSE.txt)
