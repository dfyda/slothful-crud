---
title: Validators
layout: home
parent: Fundamentals
nav_order: 2.4
---

# Validators

Slothful CRUD uses `FluentValidation` to ensure that your domain entities meet specified business rules before processing any operations. 

### Example Validator

Below is an example of how to create a validator for the `Sloth` entity using `FluentValidation`:

```csharp
public class SlothValidator : AbstractValidator<Sloth>
{
    public SlothValidator()
    {
        RuleFor(sloth => sloth.Name)
            .NotEmpty()
            .MaximumLength(255);
    }
}
```

### How It Works

If the `HasValidation` configuration is enabled, the validation is executed before performing any operations on the service of the given domain class. This ensures that only valid data is processed, preventing errors and maintaining data integrity.

{: .important }
`HasValidation` is enabled by default. If validation is enabled, you must register validators for your entities in the DI container.

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<SlothValidator>();
```

### Enabling Validation

To enable validation, ensure that the `HasValidation` property is set to `true` in your entity configuration:

```csharp
public class SlothConfiguration : ISlothEntityConfiguration<Sloth>
{
    public void Configure(SlothEntityBuilder<Sloth> builder)
    {
        builder.HasValidation(true);  // Enables validation
    }
}
```

If you do not use validators for a given entity, disable validation in configuration:

```csharp
builder.HasValidation(false);
```

By enabling validation, you ensure that all operations on your domain entities adhere to the defined business rules, improving the reliability and correctness of your application.