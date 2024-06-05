---
title: Read - browse
layout: home
parent: Endpoints
grand_parent: Fundamentals
nav_order: 2.4.3
---

# Read - browse

## Endpoint `GET /<plural-segment>/list/{page}`

Where `<plural-segment>` is the pluralized name of your domain class, e.g., for the class `Sloth`, the endpoint would be `GET /sloths/list/{page}`. The `{page}` parameter is of type `ushort` and represents the position in the paginated list.

{: .note }
Refer to the "Advanced topics/Endpoint segments" section for information on changing the way the plural segment name is generated for your API types. By default, the pluralization follows standard English rules.

### Response Codes
- **200 OK**: Returns a `PagedResults<T>` object containing the paginated list of DTOs.
- **404 Not Found**: Returns a `NotFoundResult` if no items are found.
- **400 Bad Request**: Returns a `BadRequestResult` if the request is invalid.

### Request Parameters
The browse endpoint requires the `page` parameter in the path and an `query` parameter containing fields used for filtering the browsed list of objects. These fields are public properties of the domain class in a nullable form.

### Additional Fields in Query Parameter
- **Rows**: (ushort) Specifies the number of items on the paginated page.
- **SortDirection**: (string) Specifies the sorting direction (accepted values: `asc`, `desc`).
- **SortBy**: (string) Specifies the name of the class property to sort by.

{: .important }
**DateTime Fields**: DateTime fields are converted to query parameters `DateFieldFrom` and `DateFieldTo` for filtering (condition: `DateFieldFrom >= DateField && DateFieldTo < DateField.AddDays(1)`).

{: .important }
**Nested Objects**: For nested objects, you can specify the fields for filtering and sorting in the domain class configuration. More details are available in the "Configurations / Entity Configuration" section.

### Returned Data
The endpoint returns a `PagedResults<T>` object where `T` is a DTO class created based on the public fields of the domain class.

### PagedResults<T> Class
```csharp
public class PagedResults<T> where T : new()
{
    public int First { get; }
    public int Rows { get; }
    public int Total { get; }
    public List<T> Data { get; }

    public PagedResults(int first, int total, int rows, List<T> data)
    {
        First = first;
        Total = total;
        Rows = rows;
        Data = data;
    }
}
```

- **First:** Number of skipped items.
- **Rows:** Number of fetched items.
- **Total:** Total number of items of the specified type in the system.
- **Data:** List of DTOs.

where `T` is a DTO class created based on the public fields of the domain class. It includes only the public fields of the domain class.

{: .important }
Nested Objects: By default, nested objects are transformed into a simple DTO with `Id` and `DisplayName` fields. For more detailed exposure of nested object fields, refer to the "Configurations" section.

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
GET /sloths/list/{page}
```

**Response Codes:**
- 200
- 404
- 400

**Request Parameters:**
```
page: ushort
```
```
query: {
    "rows": ushort,
    "sortDirection": string,
    "sortBy": string,
    "name": string,
    "id": guid?,
    "age": int?
}
```

**Return Value:**
```
{
    "first": 0,
    "rows": 10,
    "total": 100,
    "data": [
        {
            "id": "guid",
            "name": "string",
            "age": "integer",
            "displayName": "string"
        }
    ]
}
```

