---
title: Read - details
layout: home
parent: Endpoints
grand_parent: Fundamentals
nav_order: 2.4.2
---

# Read - details

## Endpoint `GET /<plural-segment>/{id}`

Where `<plural-segment>` is the pluralized name of your domain class, e.g., for the class `Sloth`, the endpoint would be `GET /sloths/{id}`. The `{id}` is the identifier of the element, typically of type `Guid`.

{: .note }
Refer to the "Advanced topics/Endpoint segments" section for information on changing the way the plural segment name is generated for your API types. By default, the pluralization follows standard English rules.

### Response Codes
- **200 OK**: Returns the DTO representation of the requested object.
- **404 Not Found**: Returns a `NotFoundResult` if the object is not found.
- **400 Bad Request**: Returns a `BadRequestResult` if the request is invalid.

### Request Parameters
The Read - details endpoint requires the `id` parameter in the path, which is the identifier of the element. The type of `id` is as specified in the entity configuration, defaulting to `Guid`.

**Example:**
If your domain class `Sloth` has an `id` of type `Guid`, the endpoint would look like:
```
GET /sloths/{id}
```

### Returned Data
The endpoint returns a DTO class created based on the public fields of the domain class.

{: .important }
**Nested Objects**: By default, nested objects are transformed into a simple DTO with `Id` and `DisplayName` fields. For more detailed exposure of nested object fields, refer to the "Configurations" section.

**Example Domain Class:**
```csharp
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

**Generated Endpoint:**
```
GET /sloths/{id}
```

**Response Codes:**
- 200
- 404
- 400

**Request Parameters:**
- **Path:**

```
id: guid
```

**Return Value:**
```
{
    "id": "guid",
    "name": "string",
    "age": "integer",
    "displayName": "string"
}
```