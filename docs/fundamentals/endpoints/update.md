---
title: Update
layout: home
parent: Endpoints
grand_parent: Fundamentals
nav_order: 2.4.5
---

# Update

## Endpoint `PUT /<plural-segment>/{id}`

Where `<plural-segment>` is the pluralized name of your domain class, e.g., for the class `Sloth`, the endpoint would be `PUT /sloths/{id}`. The `{id}` is the identifier of the element, typically of type `Guid`.

{: .note }
Refer to the [Endpoint segments](https://slothful.dev/advanced-topics/endpoint-segments.html) section for information on changing the way the plural segment name is generated for your API types. By default, the pluralization follows standard English rules.

### Response Codes
- **204 No Content**: Indicates that the update was successful.
- **404 Not Found**: Returns a `NotFoundResult` if the object is not found.
- **400 Bad Request**: Returns a `BadRequestResult` if the request is invalid.

### Request Parameters
The Update endpoint requires the `id` parameter in the path, which is the identifier of the element. The type of `id` is as specified in the entity configuration, defaulting to `Guid`.

The request body should contain the command with fields created based on the parameters of the data modification method in the domain class. By default, this is the `Update` method.

{: .important }
**Configuring the Update Method:** You can configure which method is used for data modification. More details can be found in the [Entity Configuration](https://slothful.dev/fundamentals/configurations/entity-configuration.html) section.

**Example:**
If your domain class `Sloth` has the following `Update` method:
```csharp
public void Update(string name, int age)
{
    Name = name;
    Age = age;
}
```

The endpoint will accept a JSON object with `name` and `age` fields in the request body.

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
PUT /sloths/{id}
```

**Response Codes:**
- 204
- 404
- 400

**Request Parameters:**
- **Path:**

```
id: guid
```
- **Body:**

```
{
    "name": "string",
    "age": "integer"
}
```