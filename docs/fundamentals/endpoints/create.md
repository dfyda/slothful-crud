---
title: Create
layout: home
parent: Endpoints
grand_parent: Fundamentals
nav_order: 2.4.1
---

# Create
## Endpoint `POST /<plural-segment>`

Where `<plural-segment>` is the pluralized name of your domain class, e.g., for the class `Sloth`, the endpoint would be `POST /sloths`.

{: .note }
Refer to the "Advanced topics/Endpoint segments" section for information on changing the way the plural segment name is generated for your API types. By default, the pluralization follows standard English rules.

### Response Codes
- **201 Created**: Returns the key property type of the newly created object.
- **400 Bad Request**: Returns a `BadRequestResult` if the request is invalid.

### Request Parameters
The Create endpoint accepts parameters based on the constructor fields of your domain class. 

**Example:**
If your domain class `Sloth` has the following constructor:
```csharp
public Sloth(Guid id, string name, int age)
{
    Id = id;
    Name = name;
    Age = age;
}
```

The endpoint will accept a JSON object with `id`, `name`, and `age` fields.

{: .important }
Nested Objects: If your class contains a nested object and you wish to add it, you should specify the field representing the identifier of that nested object, e.g., YourNestedObjectId.

**Return Value:** The endpoint returns the identifier of the newly created object.

{: .note }
Refer to the "Advanced topics/Constructor" section for information on specifying a particular constructor. By default, the first constructor with parameters is used.

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
POST /sloths
```

**Response Codes:**
- 201
- 400

**Request Parameters:**
- **Body:**

```
{
    "id": "guid",
    "name": "string",
    "age": "integer"
}
```

**Return Value:**
```
{
    "id": "guid-of-new-sloth"
}
```