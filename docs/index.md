---
title: Quick start
layout: home
nav_order: 1
---

<div align="center">
    <img src="assets/slothful-api-doc.jpg" alt="slothful-crud logo">
</div>

# Relax and Generate CRUD Endpoints
{: .fs-9 }

Slothful CRUD is a library designed to streamline the creation of CRUD endpoints effortlessly. By implementing the necessary interfaces in domain classes, you can quickly register the library and generate RESTful endpoints. Simplify your API development with Slothful CRUD.
{: .fs-6 .fw-300 }

---

## Getting Started

Before you can use Slothful CRUD in your application, you need to add the NuGet package. You can do it using your IDE or the command line

```
dotnet add package slothful-crud
```

## Basic Usage
To configure and use Slothful CRUD in your application, follow these steps.

### Modify `Program.cs`
##### Step 1: Add Services
In the `Program.cs` file, add Slothful CRUD services to the dependency injection container.

```csharp
using SlothfulCrud.Extensions;

var builder = WebApplication.CreateBuilder(args);
// Add SlothfulCrud to the DI container
builder.Services.AddSlothfulCrud<SlothfulDbContext, Program>();

var app = builder.Build();
```
In the above code, `SlothfulDbContext` is your database context class that should inherit from `DbContext` and be defined in your project. `Program` serves as an assembly marker — you can use any type from the assembly containing your entities.

##### Step 2: Configure the Application
Next, configure the application to use Slothful CRUD by calling the `UseSlothfulCrud` extension method:

```csharp
// Configure the application to use SlothfulCrud
app.UseSlothfulCrud<SlothfulDbContext, Program>();

app.Run();
```

{: .important }
Use the generic overload `UseSlothfulCrud<TDbContext, TAssemblyMarker>()` to register generated endpoints.  
The non-generic overload `UseSlothfulCrud()` does not register endpoints.

{: .note }
The legacy single-generic overloads `AddSlothfulCrud<TDbContext>()` and `UseSlothfulCrud<TDbContext>()` are deprecated. They rely on `Assembly.GetEntryAssembly()` which may not work correctly in test or hosted scenarios. Use the two-generic overloads with an assembly marker instead.

### Define `DbContext`
Ensure you have defined your database context `DbContext` like this:

```csharp
using Microsoft.EntityFrameworkCore;

public class SlothfulDbContext : DbContext
{
    public SlothfulDbContext(DbContextOptions<SlothfulDbContext> options) : base(options)
    {
    }

    public DbSet<Sloth> Sloths { get; set; }
}
```

### Implementing the `ISlothfulEntity` Interface
To use Slothful CRUD with your entities, you need to implement the `ISlothfulEntity` interface. Here is an example implementation:

```csharp
using SlothfulCrud.Domain;

namespace SlothfulApp.Api.Domain
{
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
}
```

In this example:

- The `Sloth` class implements the `ISlothfulEntity` interface.
- The `Id`, `Name`, and `Age` properties are defined for the `Sloth` entity.
- The `DisplayName` property is implemented from the `ISlothfulEntity` interface.
- The `Sloth` class includes constructors and an `Update` method for managing entity data.

**Explanation of Properties and Methods**
- **DisplayName Property:** The `DisplayName` property is used to present the entity in a simplified form, similar to a Data Transfer Object (DTO) with properties `Id` and `DisplayName`. This helps in displaying concise information about the entity.
- **Constructor:** The constructor is used to create parameters for the POST Create endpoint. When a new entity is created, the constructor initializes the necessary properties.
- **Update Method:** The `Update` method is used to create parameters for the PUT Update endpoint. This method allows updating the entity's properties with new values.

## Generated Endpoints

Slothful CRUD automatically generates several endpoints for managing your entities. Below is a summary of the 6 endpoints generated for the `Sloth` entity:

## Performance Benchmarking

Slothful CRUD includes BenchmarkDotNet benchmarks for regression detection and performance validation.

Run benchmarks locally:

```bash
dotnet run -c Release --project library/SlothfulCrud/Tests/SlothfulCrud.Benchmarks -- --filter "*"
```

Artifacts are generated in:

`library/SlothfulCrud/Tests/SlothfulCrud.Benchmarks/BenchmarkDotNet.Artifacts/results/`

### Get Sloth Details

##### Endpoint `GET /sloths/{id}`
#### Description
Retrieves the details of a specific sloth by its ID.

#### Parameters
- `id` (path, required): UUID of the sloth.

#### Responses
- `200`: Success, returns `SlothDetailsDto`.
- `404`: Not Found.
- `400`: Bad Request.

#### Response Schema: `SlothDetailsDto`

- **id**: UUID of the sloth.
- **name**: Name of the sloth (nullable).
- **age**: Age of the sloth (integer).
- **displayName**: Display name of the sloth (nullable).

### Update Sloth

##### Endpoint `PUT /sloths/{id}`

#### Description
Updates the details of a specific sloth by its ID.

#### Parameters
- `id` (path, required): UUID of the sloth.

#### Request Body
- `UpdateSloth`: JSON schema with updated sloth details.

#### Responses
- `204`: No Content.
- `404`: Not Found.
- `400`: Bad Request.

#### Request Schema: `UpdateSloth`

- **name**: Updated name of the sloth (nullable).
- **age**: Updated age of the sloth (integer).

### Delete Sloth

##### Endpoint `DELETE /sloths/{id}`
#### Description
Deletes a specific sloth by its ID.

#### Parameters
- `id` (path, required): UUID of the sloth.

#### Responses
- `204`: No Content.
- `404`: Not Found.
- `400`: Bad Request.

### Browse Sloths

##### Endpoint `GET /sloths/list/{page}`

#### Description
Retrieves a paginated list of sloths.

#### Parameters
- `page` (path, required): Page number (integer).
- `query` (query, optional): Additional query parameters.

#### Responses
- `200`: Success, returns `SlothDtoPagedResults`.
- `400`: Bad Request.

#### Response Schema: `SlothDtoPagedResults`

- **first**: The first item index in the current page (integer).
- **rows**: Number of rows requested for a single page.
- **total**: The total number of items (integer).
- **data**: Array of `SlothDto` objects (nullable).

#### Schema: `SlothDto`

- **id**: UUID of the sloth.
- **name**: Name of the sloth (nullable).
- **age**: Age of the sloth (integer).
- **displayName**: Display name of the sloth (nullable).

### Browse Selectable Sloths

##### Endpoint `GET /sloths/selectable-list/{page}`

#### Description
Retrieves a paginated list of selectable sloths.

#### Parameters
- `page` (path, required): Page number (integer).
- `query` (query, optional): Additional query parameters.

#### Responses
- `200`: Success, returns `BaseEntityDtoPagedResults`.
- `400`: Bad Request.

#### Response Schema: `BaseEntityDtoPagedResults`

- **first**: The first item index in the current page (integer).
- **rows**: Number of rows requested for a single page.
- **total**: The total number of items (integer).
- **data**: Array of `BaseEntityDto` objects (nullable).

#### Schema: `BaseEntityDto`

- **id**: UUID of the entity (nullable).
- **displayName**: Display name of the entity (nullable).

### Create Sloth

##### Endpoint `POST /sloths`

#### Description
Creates a new sloth.

#### Request Body
- `CreateSloth`: JSON schema with new sloth details.

#### Responses
- `201`: Created, returns the UUID of the newly created sloth.
- `400`: Bad Request.

On `201`, the `Location` header points to the created resource details path: `/{segment}/{id}` (segment is resolved by `IApiSegmentProvider`).

#### Request Schema: `CreateSloth`

- **name**: Name of the new sloth (nullable).
- **age**: Age of the new sloth (integer).

`id` is generated server-side by the configured key value provider and is not part of the create request body.

