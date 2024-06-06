---
title: Swagger
layout: home
parent: Fundamentals
nav_order: 2.5
---

# Swagger

Slothful CRUD provides seamless integration with Swagger to generate interactive API documentation for your endpoints. This section outlines how to configure and utilize Swagger in your application.

### Configuration

To enable Swagger in your application, add the following lines to your `Program.cs` file:

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

### Usage

By enabling Swagger, you provide a powerful tool for developers to understand and interact with your API. It facilitates testing, debugging, and integrating your API with other systems.

For more advanced customization options, refer to the [Swagger documentation](https://swagger.io/docs/).

### Example

Here is an example of the Swagger documentation generated for a typical Slothful CRUD endpoint:

#### Get Sloth Details

- **Endpoint:** `GET /sloths/{id}`
- **Parameters:**
  - `id` (path, required): The unique identifier of the sloth (UUID).
- **Responses:**
  - `200`: Success, returns `SlothDetailsDto`.
    - **Schema:**
      ```json
      {
        "id": "string (uuid)",
        "name": "string",
        "age": "integer",
        "displayName": "string"
      }
      ```
  - `404`: Not Found.
    - **Schema:**
      ```json
      {
        "statusCode": 404
      }
      ```
  - `400`: Bad Request.
    - **Schema:**
      ```json
      {
        "statusCode": 400
      }
      ```

This example demonstrates the detailed information provided by Slothful CRUD in the Swagger documentation, making it easier for developers to understand the API's functionality and how to interact with it effectively.

{: .note }
Integrating Swagger with Slothful CRUD enhances your API's usability by providing comprehensive and interactive documentation. This ensures that developers can easily explore and understand the capabilities of your API.