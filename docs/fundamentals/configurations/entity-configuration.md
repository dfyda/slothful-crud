---
title: Entity configuration
layout: home
parent: Configurations
grand_parent: Fundamentals
nav_order: 2.2.1
---

# Entity configuration

The following configuration options are available for customizing the behavior of your entities in the Slothful CRUD library:

- **ExposeAllNestedProperties (bool):** Set to `true` to expose all nested properties via the API. This is useful for debugging or gaining granular control over nested data.
- **IsAuthorizationEnable (bool):** Determines whether authorization checks are enabled. If set to `true`, the policies listed in `PolicyNames` are applied.
- **PolicyNames (string[]):** An array of policy names to apply for authorization.
- **SortProperty (string):** Specifies the property name used to sort entities.
- **FilterProperty (string):** Identifies the property name used to filter entities.
- **KeyProperty (string):** Defines the primary key property name for identifying entities.
- **KeyPropertyType (Type):** Indicates the type of the primary key property (e.g., `int`, `Guid`).
- **UpdateMethod (string):** Specifies the method used to update entities.
- **HasValidation (bool):** Enables or disables validation for entities to ensure compliance with business rules.

### Methods
- **AllowAnonymous:** Disables authorization checks for the entity.
- **RequireAuthorization:** Enables authorization checks and applies the specified policies.
- **SetSortProperty:** Sets the property used for sorting entities.
- **SetFilterProperty:** Sets the property used for filtering entities.
- **SetKeyProperty:** Sets the key property and also sets the key property type.
- **SetKeyPropertyType:** Sets the type of the key property.
- **SetUpdateMethodName:** Sets the method name used to update entities.
- **ExposeAllNestedProperties:** Exposes all nested properties.

{: .important }
Calling `SetKeyProperty` also invokes `SetKeyPropertyType` to ensure consistency.

### Example Usage

Below is an example configuration using default values:

```csharp
public class SlothConfiguration : ISlothEntityConfiguration<Sloth>
{
    public void Configure(SlothEntityBuilder<Sloth> builder)
    {
        builder
            .SetExposeAllNestedProperties(false)  // Default value
            .SetIsAuthorizationEnable(false)      // Default value
            .SetPolicyNames(Array.Empty<string>()) // Default value
            .SetSortProperty(x => x.Name)          // Default value
            .SetFilterProperty(x => x.Name)        // Default value
            .SetUpdateMethodName("Update")         // Default value
            .SetKeyProperty(x => x.Id)             // Default value
            .SetKeyPropertyType(typeof(Guid))      // Default value
            .SetHasValidation(true);               // Default value
    }
}
```

These settings illustrate the use of default values for the entity configuration in Slothful CRUD. This is equivalent to:

```csharp
public class SlothConfiguration : ISlothEntityConfiguration<Sloth>
{
    public void Configure(SlothEntityBuilder<Sloth> builder)
    {
    }
}
```

