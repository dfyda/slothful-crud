<div align="center">
    <img src="docs/assets/slothful-api.jpg" alt="slothful-api logo">
</div>

Slothful CRUD
===========
Slothful CRUD is a library designed to streamline the creation of CRUD endpoints effortlessly. By implementing the necessary interfaces in domain classes, you can quickly register the library and generate RESTful endpoints. Simplify your API development with Slothful CRUD.

## Documentation

Get started by [reading through the documentation](https://slothful.dev/).

## Getting Started

Follow these steps to integrate Slothful CRUD into your project:

1. **Configure Your Domain Classes:**

Create domain classes representing your data entities.
Implement the appropriate interfaces for CRUD operations.

2. **Register the Library:**

Integrate Slothful API by registering it in your application.

3. **Run Your Application:**

Start your application to automatically generate RESTful endpoints based on your domain classes.

## Configuration
The base configuration class that defines shared settings for all other configuration classes:

- **ExposeAllNestedProperties** (`bool`): Set to `true` to expose all nested properties via the API. Useful for debugging or gaining granular control over nested data.
- **IsAuthorizationEnable** (`bool`): Determines whether authorization checks are enabled. If set to `true`, the policies listed in `PolicyNames` are applied.
- **PolicyNames** (`string[]`): An array of policy names to apply for authorization.

### `EndpointConfiguration` (inherits `Configuration`)
Defines properties specific to API endpoints:

- **IsEnable** (`bool`): Determines if the endpoint is enabled.

### `EntityConfiguration` (inherits `Configuration`)
Provides settings specific to entity management:

- **SortProperty** (`string`): Specifies the property name used to sort entities.
- **FilterProperty** (`string`): Identifies the property name used to filter entities.
- **KeyProperty** (`string`): Defines the primary key property name for identifying entities.
- **KeyPropertyType** (`Type`): Indicates the type of the primary key property (e.g., `int`, `string`).
- **UpdateMethod** (`string`): Specifies the method used to update entities.
- **HasValidation** (`bool`): Enables or disables validation for entities to ensure compliance with business rules.