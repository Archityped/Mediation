# Archityped.Mediation.Abstractions

**Archityped.Mediation.Abstractions** defines the core interfaces and message contracts for the [Archityped.Mediation](https://www.nuget.org/packages/Archityped.Mediation) library.

This package is designed for modeling mediator interactions within **shared**, **domain**, or **decoupled** projects, promoting clean boundaries and testable components while staying independent of runtime implementation details.

It includes **built-in source generation** for **compile-time type inference**, enabling calls such as:

```csharp
Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request);
```

to be resolved from a **single strongly-typed parameter**:

```csharp
Task<TResponse> SendAsync(IRequest<TResponse> request);
```

This simplifies usage and provides **NativeAOT compatibility** without relying on runtime reflection.

---

## 🧩 Key Features

- **Mediator Abstraction**  
  - `IMediator` – Unified entry point for sending requests, publishing notifications, and streaming responses  
  - `IRequestSender` – Abstraction for dispatching requests

- **Request/Response Contracts**  
  - `IRequest<TResponse>` – Marker for requests expecting a typed result  
  - `IRequestHandler<TRequest, TResponse>` – Handles requests expecting a response
  - `IRequest` – Marker for void-returning requests  
  - `IRequestHandler<TRequest>` – Handles fire-and-forget requests  

- **Notification Publishing**  
  - `INotification` – Marker for notifications  
  - `INotificationHandler<TNotification>` – Handles published notifications  
  - `INotificationPublisher` – Abstraction for dispatching notifications to multiple handlers  

- **Streaming Requests**  
  - `IStreamRequest<TResponse>` – Marker for requests streaming a typed result
  - `IStreamRequestHandler<TRequest, TResponse>` – Handles requests streaming a response

- **Middleware Contracts**  
  - `IRequestMiddleware` – Middleware pipeline for request/response  
  - `IStreamRequestMiddleware` – Middleware pipeline for stream requests  

---

## 🚀 Getting Started

**Archityped.Mediation** defines three core message types:

- **Query (`IRequest<TResponse>`)** – Read-only operation that returns data.  
- **Command (`IRequest` or `IRequest<TResponse>`)** – Operation that performs work, optionally returning a result.  
- **Notification (`INotification`)** – Broadcast message sent to zero or more handlers.

These contracts are implemented in **Archityped.Mediation.Abstractions**, keeping domain code independent of runtime concerns.

### Queries

Queries represent **read-only operations** that retrieve data without modifying state. They implement `IRequest<TResponse>` and are handled by `IRequestHandler<TRequest, TResponse>`.

```csharp
public record GetUserByEmail(string Email) : IRequest<User?>;

public class GetUserByEmailHandler : IRequestHandler<GetUserByEmail, User?>
{
    private readonly IUserRepository _users;
    public GetUserByEmailHandler(IUserRepository users) => _users = users;

    public Task<User?> HandleAsync(GetUserByEmail request, CancellationToken ct = default) =>
        _users.FindByEmailAsync(request.Email, ct);
}
```

### Commands

Commands represent **units of work** that perform actions, such as changing state or coordinating processes.

#### **With Response**

The command performs work and returns a simple value, such as the ID of the new resource.

```csharp
public record CreateUser(string Email) : IRequest<Guid>;

public class CreateUserHandler : IRequestHandler<CreateUser, Guid>
{
    private readonly IUserRepository _users;
    public CreateUserHandler(IUserRepository users) => _users = users;

    public async Task<Guid> HandleAsync(CreateUser request, CancellationToken ct = default)
    {
        var id = Guid.NewGuid();
        await _users.AddAsync(new User(id, request.Email), ct);
        return id;
    }
}
```

#### **Without Response**

The command performs work but does not return data.

```csharp
public record CreateUser(string Email) : IRequest;

public class CreateUserHandler : IRequestHandler<CreateUser>
{
    private readonly IUserRepository _users;
    public CreateUserHandler(IUserRepository users) => _users = users;

    public Task HandleAsync(CreateUser request, CancellationToken ct = default) =>
        _users.AddAsync(new User(Guid.NewGuid(), request.Email), ct);
}
```

### Notifications

Notifications represent **fire-and-forget messages** that signal something has happened. They implement `INotification` and are processed by any number of `INotificationHandler<TNotification>` implementations.

```csharp
public record UserRegistered(Guid Id, string Email) : INotification;

public class SendWelcomeEmailHandler : INotificationHandler<UserRegistered>
{
    private readonly IEmailService _email;
    public SendWelcomeEmailHandler(IEmailService email) => _email = email;

    public Task HandleAsync(UserRegistered notification, CancellationToken ct = default) =>
        _email.SendAsync(notification.Email, "Welcome!", "Thanks for signing up!", ct);
}

public class LogUserRegistrationHandler : INotificationHandler<UserRegistered>
{
    private readonly IEventLogger _logger;
    public LogUserRegistrationHandler(IEventLogger logger) => _logger = logger;

    public Task HandleAsync(UserRegistered notification, CancellationToken ct = default)
    {
        _logger.WriteInfo($"User registered: {notification.Email}");
        return Task.CompletedTask;
    }
}
```

Publishing a `UserRegistered` notification will invoke both handlers independently, without coupling them to the command.

### Streaming

Streaming requests represent operations that return data progressively over time.  
They implement `IStreamRequest<TResponse>` and are handled by `IStreamRequestHandler<TRequest, TResponse>`.

```csharp
public record GetRecentOrders(int CustomerId) : IStreamRequest<Order>;

public class GetRecentOrdersHandler : IStreamRequestHandler<GetRecentOrders, Order>
{
    private readonly IOrderRepository _orders;
    public GetRecentOrdersHandler(IOrderRepository orders) => _orders = orders;

    public async IAsyncEnumerable<Order> HandleAsync(GetRecentOrders request, [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var order in _orders.GetRecentOrdersAsync(request.CustomerId, ct))
            yield return order;
    }
}
```

### Middleware

Middleware allows you to handle cross-cutting concerns such as logging, validation, metrics, caching, or authorization before and after requests are handled.

To create middleware, implement `IRequestMiddleware` or `IStreamRequestMiddleware`.

#### Example: Logging Middleware

```csharp
public class LoggingMiddleware : IRequestMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(IRequestContext context, RequestDelegate next, CancellationToken ct)
    {
        _logger.LogInformation("Handling {RequestType}", context.Request.GetType().Name);

        await next(context, ct);

        _logger.LogInformation("Handled {RequestType}", context.Request.GetType().Name);
    }
}
```

The `LoggingMiddleware` will run for **every request**, before and after its handler executes.

### Summary

| Type            | Interface                    | Returns Data   | Purpose                       |
|-----------------|------------------------------|----------------|-------------------------------|
| **Query**       | `IRequest<TResponse>`        | ✅ Yes         | Retrieve data                  |
| **Command**     | `IRequest` or `IRequest<TResponse>` | Optional        | Perform work, optionally return a result |
| **Notification**| `INotification`              | ❌ No          | Notify subscribers             |
| **Streaming**   | `IStreamRequest<TResponse>`  | ✅ Yes         | Stream results progressively   |
| **Middleware**  | `IRequestMiddleware` / `IStreamRequestMiddleware` | N/A            | Intercept requests for cross-cutting concerns (e.g., logging, validation, metrics) |

This code depends only on **abstractions**.

At runtime, the [Archityped.Mediation](https://www.nuget.org/packages/Archityped.Mediation) package provides:

- **Handler Resolution** – Discovering and invoking the appropriate handlers through the service provider.  
- **Dispatching** – Executes commands, queries, notifications, and streaming requests.  
- **Middleware** – Executes cross-cutting behaviors such as logging, validation, and authorization.

---

## 📚 Learn More

- Runtime implementation: [Archityped.Mediation](https://www.nuget.org/packages/Archityped.Mediation)

---

© [Archityped](https://github.com/Archityped) _– Building types to perfection_
