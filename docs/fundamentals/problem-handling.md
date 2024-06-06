---
title: Problem handling
layout: home
parent: Fundamentals
nav_order: 2.3
---

# Problem handling

Slothful CRUD provides robust problem handling to manage exceptions and errors in a structured manner. This section outlines how to enable and configure problem handling within your application.

### Enabling Problem Handling

To enable problem handling in your application, you need to configure it in your `Program.cs` file:

```csharp
app.UseSlothfulCrud<SlothfulDbContext>(options => options.UseSlothfulProblemHandling = true);
```

This setting enables the middleware that catches exceptions and processes them through a centralized handler.

### How It Works

When an exception occurs, the middleware catches it and passes it to the `ExceptionHandler`, which processes the exception and returns a standardized response. This response is formatted as a `SlothProblemDetails` object and includes details about the error, such as the problem ID, status code, and a human-readable title and message.

### Exception Types

The `ExceptionHandler` can handle several types of exceptions, each mapped to specific HTTP status codes and messages:

- **NotFoundException:** Returns a 404 Not Found status with details about the missing resource.
- **ValidationException:** Returns a 400 Bad Request status with information about validation errors.
- **ConfigurationException:** Returns a 403 Forbidden status indicating a configuration issue.
- **AuthenticationException:** Returns a 401 Unauthorized status when authentication fails.
- **UnauthorizedAccessException:** Returns a 403 Forbidden status indicating access is not allowed.
- **General Exceptions:** Returns a 500 Internal Server Error status for unhandled exceptions.

### Customizing Problem Responses

The `ExceptionHandler` logs critical errors and creates a `SlothProblemDetails` response that includes:

- **ProblemId:** A unique identifier for the problem instance.
- **Title:** A human-readable title derived from the exception code.
- **Detail:** A detailed message describing the error.
- **Code:** A specific error code.
- **Status:** The HTTP status code associated with the error.

For validation errors, additional information about the validation failures is included in the response.

### Middleware Components

The problem handling consists of the following key components:

- **ExceptionMiddleware:** This middleware intercepts exceptions, processes them using the `ExceptionHandler`, and formats the response.
- **ExceptionHandler:** This service handles the logic for processing exceptions and creating standardized problem responses.

{: .note }
For additional information on how to replace exception handling with your own implementation, refer to the "Advanced topics/Exceptions" section.
