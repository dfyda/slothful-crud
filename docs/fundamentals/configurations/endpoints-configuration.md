---
title: Endpoints configuration
layout: home
parent: Configurations
grand_parent: Fundamentals
nav_order: 2.2.2
---

# Endpoints configuration

The following configuration options are available for customizing the behavior of your endpoints in the Slothful CRUD library:

- **HasEndpoint (bool):** Enables or disables the endpoint.
- **ExposeAllNestedProperties (bool):** Set to `true` to expose all nested properties via the API. This is useful for debugging or gaining granular control over nested data.
- **IsAuthorizationEnable (bool):** Determines whether authorization checks are enabled. If set to `true`, the policies listed in `PolicyNames` are applied.
- **PolicyNames (string[]):** An array of policy names to apply for authorization.

### Methods
- **HasEndpoint:** Enables or disables the endpoint.
- **AllowAnonymous:** Disables authorization checks for the endpoint.
- **RequireAuthorization:** Enables authorization checks and applies the specified policies.
- **ExposeAllNestedProperties:** Exposes all nested properties for the endpoint.

### Example Usage

Below is an example configuration using default values:

```csharp
public class SlothEndpointConfiguration : ISlothEndpointConfiguration<Sloth>
{
    public void Configure(SlothEndpointBuilder<Sloth> builder)
    {
        builder.GetEndpoint
            .HasEndpoint(true)                     // Default value
            .ExposeAllNestedProperties(false)      // Default value
            .AllowAnonymous();                     // Default value

        builder.BrowseEndpoint
            .HasEndpoint(true)                     // Default value
            .ExposeAllNestedProperties(false)      // Default value
            .AllowAnonymous();                     // Default value

        builder.BrowseSelectableEndpoint
            .HasEndpoint(true)                     // Default value
            .ExposeAllNestedProperties(false)      // Default value
            .AllowAnonymous();                     // Default value

        builder.CreateEndpoint
            .HasEndpoint(true)                     // Default value
            .ExposeAllNestedProperties(false)      // Default value
            .AllowAnonymous();                     // Default value

        builder.UpdateEndpoint
            .HasEndpoint(true)                     // Default value
            .ExposeAllNestedProperties(false)      // Default value
            .AllowAnonymous();                     // Default value

        builder.DeleteEndpoint
            .HasEndpoint(true)                     // Default value
            .ExposeAllNestedProperties(false)      // Default value
            .AllowAnonymous();                     // Default value
    }
}
```

These settings illustrate the use of default values for the endpoint configuration in Slothful CRUD. This is equivalent to:

```csharp
public class SlothEndpointConfiguration : ISlothEndpointConfiguration<Sloth>
{
    public void Configure(SlothEndpointBuilder<Sloth> builder)
    {
    }
}
```