---
title: Endpoint segments
layout: home
parent: Advanced topics
nav_order: 3.2
---

# Endpoint segments

The Slothful CRUD library allows you to customize the API segments used for your endpoints by implementing the `IApiSegmentProvider` interface.

### Default Implementation

By default, the `ApiSegmentProvider` class is used, which converts the entity name to its plural form and then transforms it from camel case to hyphenated case.

### Customizing the API Segment
To customize the API segment, you can create your own implementation of the `IApiSegmentProvider` interface and register it in the DI container.

```csharp
public interface IApiSegmentProvider
{
    string GetApiSegment(string entityName);
}
```

### Importance of Customizing API Segments
Customizing the API segments allows you to:

- **Maintain Naming Conventions:** Ensure that your API endpoint names follow specific naming conventions required by your application or organization.
- **Improve Readability:** Make the API segments more readable and intuitive for clients consuming the API.
- **Integrate with Existing Systems:** Align the API segments with existing systems and routes in your application.