---
title: Read - browse selectable
layout: home
parent: Endpoints
grand_parent: Fundamentals
nav_order: 2.4.4
---

# Read - browse selectable

## Endpoint `GET /<plural-segment>/selectable-list/{page}`

Where `<plural-segment>` is the pluralized name of your domain class, e.g., for the class `Sloth`, the endpoint would be `GET /sloths/selectable-list/{page}`. The `{page}` parameter is of type `ushort` and represents the position in the paginated list.

This endpoint is used to support selectable lists in dropdown controls with backend search functionality.

{: .note }
Refer to the "Advanced topics/Endpoint segments" section for information on changing the way the plural segment name is generated for your API types. By default, the pluralization follows standard English rules.

### Response Codes
- **200 OK**: Returns a `PagedResults<BaseEntityDto>` object containing the paginated list of selectable DTOs.
- **404 Not Found**: Returns a `NotFoundResult` if no items are found.
- **400 Bad Request**: Returns a `BadRequestResult` if the request is invalid.

### Request Parameters
The browse selectable endpoint requires the `page` parameter in the path and an `query` parameter containing fields used for filtering and sorting the browsed list of objects. The query parameter includes:

- **Search**: Specific field for filtering the objects.
- **Rows**: (ushort) Specifies the number of items on the paginated page.
- **SortDirection**: (string) Specifies the sorting direction (accepted values: `asc`, `desc`).
- **SortBy**: (string) Specifies the name of the class property to sort by.

{: .important }
**Field for Filtering**: You need to configure the field for filtering this type of object, which is compared to the `Search` field from the query. Refer to the "Configurations/Entity Configuration" section for more details.

### Returned Data
The endpoint returns a `PagedResults<BaseEntityDto>` object where `BaseEntityDto` is a class containing `Id` and `DisplayName` fields.

### PagedResults<BaseEntityDto> Class
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

where `T` is the `BaseEntityDto` class:
```csharp
public class BaseEntityDto
{
    public object Id { get; set; }
    public string DisplayName { get; set; }
}
```

- **First:** Number of skipped items.
- **Rows:** Number of fetched items.
- **Total:** Total number of items of the specified type in the system.
- **Data:** List of BaseEntityDto.

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
GET /sloths/selectable-list/{page}
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
    "search": string,
    "rows": ushort,
    "sortDirection": string,
    "sortBy": string
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
            "displayName": "string"
        }
    ]
}
```


