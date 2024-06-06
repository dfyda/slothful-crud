---
title: Constructors
layout: home
parent: Advanced topics
nav_order: 3.1
---

# Constructors

You can change the configuration of the constructor used for the Create endpoint by overriding the implementation and registering it in the DI container.

```csharp
public interface ICreateConstructorBehavior
{
    ConstructorInfo GetConstructorInfo(Type entityType);
}
```

### How to Override
To override the default behavior, you need to implement the `ICreateConstructorBehavior` interface and register it in your DI container.

### Default Behavior

By default, the implementation selects the first constructor that has parameters.

### Importance of Constructor Configuration

Configuring the constructor is important because it allows you to control which constructor is used during the creation of new entities. This is particularly useful in scenarios where:

- **Multiple Constructors:** Your entity class has multiple constructors, and you need to specify which one to use.
- **Custom Logic:** You need to apply custom logic to determine the appropriate constructor based on the entity type.
- **Dependency Injection:** You want to ensure that the selected constructor works well with other dependencies in your system.

By customizing the constructor behavior, you can achieve greater control and flexibility over how new instances of your entities are created, ensuring that they are initialized correctly according to your business rules and requirements.

{: .note }
The default implementation selects the first constructor that has parameters. If this default behavior meets your needs, no further configuration is necessary.
