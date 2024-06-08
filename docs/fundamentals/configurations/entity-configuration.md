---
title: Entity configuration
layout: home
parent: Configurations
grand_parent: Fundamentals
nav_order: 2.2.1
---

# Entity configuration

The following configuration options are available for customizing the behavior of your entities in the Slothful CRUD library:

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
            .ExposeAllNestedProperties(false)  // Default value
            .SetSortProperty(x => x.Name)          // Default value
            .SetFilterProperty(x => x.Name)        // Default value
            .SetUpdateMethodName("Update")         // Default value
            .SetKeyProperty(x => x.Id)             // Default value
            .SetKeyPropertyType(x => x.Id)      // Default value
            .HasValidation();               // Default value
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

