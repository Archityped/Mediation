# Archityped.Mediation

**Archityped.Mediation** is a modern, high-performance mediation library for .NET.  
It unifies in-process messaging, CQRS, and event-driven patterns under a clean abstraction — designed to be composable, extensible, and NativeAOT-friendly.

This package provides the **runtime implementation**, including concrete `IMediator` services, handler resolution, middleware pipelines, and dispatching of commands, queries, notifications, and streaming requests.

---

## 🧩 Key Features

- **Mediator Implementation**  
  - `IMediator` – Unified entry point for sending requests, publishing notifications, and streaming responses  
  - `IRequestSender` – Dispatch abstraction that flows through middleware pipelines

- **Request/Response Handling**  
  - `IRequest<TResponse>` – Marker for requests expecting a typed result  
  - `IRequestHandler<TRequest, TResponse>` – Handles requests expecting a response  
  - `IRequest` – Marker for void-returning requests  
  - `IRequestHandler<TRequest>` – Handles fire-and-forget requests  

- **Notification Publishing**  
  - `INotification` – Marker for notifications  
  - `INotificationHandler<TNotification>` – Handles published notifications  
  - `INotificationPublisher` – Dispatches notifications to multiple handlers  

- **Streaming Requests**  
  - `IStreamRequest<TResponse>` – Marker for requests streaming a typed result  
  - `IStreamRequestHandler<TRequest, TResponse>` – Handles requests streaming a response

- **Middleware Pipelines**  
  - `IRequestMiddleware` – Middleware pipeline for request/response  
  - `IStreamRequestMiddleware` – Middleware pipeline for streaming requests  

- **Dependency Injection Friendly**  
  Built for `Microsoft.Extensions.DependencyInjection` with modular registration, assembly scanning, and configuration hooks.

---

## 🚀 Getting Started

**Archityped.Mediation** supports four core message types:

- **Query (`IRequest<TResponse>`)** – Read-only operation that returns data.  
- **Command (`IRequest` or `IRequest<TResponse>`)** – Operation that performs work, optionally returning a result.  
- **Notification (`INotification`)** – Broadcast message sent to zero or more handlers.  
- **Streaming (`IStreamRequest<TResponse>`)** – Operation that streams results progressively.

These message type contracts are defined in  
[Archityped.Mediation.Abstractions](https://www.nuget.org/packages/Archityped.Mediation.Abstractions).  
This package provides the **runtime implementations** needed to execute them.

### Queries

Queries represent **read-only operations** that retrieve data without modifying state.  
They implement `IRequest<TResponse>` and are handled by `IRequestHandler<TRequest, TResponse>`.

```csharp
public record GetUserByEmail(string Email) : IRequest<User?>;

public class GetUserByEmailHandler : IRequestHandler<GetUserByEmail, User?>
{
    private readonly IUserRepository _users;
    public GetUserByEmailHandler(IUserRepository users) => _users = users;

    public Task<User?> HandleAsync(GetUserByEmail request, CancellationToken ct = default) =>
        _users.FindByEmailAsync(request.Email, ct);
}

// Registration with an instance of IServiceCollection
services.AddMediator(cfg =>
{
    cfg.AddRequestHandler<GetUserByEmailHandler>();
});

// Using SendAsync
var mediator = provider.GetRequiredService<IMediator>();
var user = await mediator.SendAsync(new GetUserByEmail("alice@example.com"));
```

### Commands

Commands represent **units of work** that perform actions, such as changing state or coordinating processes.  
They may either return no data or return a simple value, like an identifier or status.

#### **With Response**

The command performs work and returns a simple value, such as the ID of a newly created resource.

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

// Registration with an instance of IServiceCollection
services.AddMediator(cfg =>
{
    cfg.AddRequestHandler<CreateUserHandler>();
});

// Using SendAsync
var newUserId = await mediator.SendAsync(new CreateUser("bob@example.com"));
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

// Registration with an instance of IServiceCollection
services.AddMediator(cfg =>
{
    cfg.AddRequestHandler<CreateUserHandler>();
});

// Using SendAsync
await mediator.SendAsync(new CreateUser("bob@example.com"));
```

### Notifications

Notifications represent **fire-and-forget messages** that signal something has happened.  
They implement `INotification` and are processed by any number of `INotificationHandler<TNotification>` implementations.

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

// Registration with an instance of IServiceCollection
services.AddMediator(cfg =>
{
    cfg.AddNotificationHandler<SendWelcomeEmailHandler>()
       .AddNotificationHandler<LogUserRegistrationHandler>();
});

// Using PublishAsync
await mediator.PublishAsync(new UserRegistered(Guid.NewGuid(), "bob@example.com"));
```

Publishing a `UserRegistered` notification will invoke both handlers independently, without coupling them to the command.

---

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

// Registration with an instance of IServiceCollection
services.AddMediator(cfg =>
{
    cfg.AddStreamRequestHandler<GetRecentOrdersHandler>();
});

// Using StreamAsync
await foreach (var order in mediator.StreamAsync(new GetRecentOrders(customerId: 42)))
{
    Console.WriteLine($"Order ID: {order.Id}, Total: {order.Total:C}");
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

// Registration with an instance of IServiceCollection
services.AddMediator(cfg =>
{
    cfg.AddRequestMiddleware<LoggingMiddleware>();
});
```

The `LoggingMiddleware` will run for **every request**, before and after its handler executes. You can add multiple middleware components, and they will run in the order they are registered.


### Summary

| Type            | Interface                    | Returns Data   | Purpose                       |
|-----------------|------------------------------|----------------|-------------------------------|
| **Query**       | `IRequest<TResponse>`        | ✅ Yes         | Retrieve data                  |
| **Command**     | `IRequest` or `IRequest<TResponse>` | Optional        | Perform work, optionally return a result |
| **Notification**| `INotification`              | ❌ No          | Notify subscribers             |
| **Streaming**   | `IStreamRequest<TResponse>`  | ✅ Yes         | Stream results progressively   |
| **Middleware**  | `IRequestMiddleware` / `IStreamRequestMiddleware` | N/A            | Intercept requests for cross-cutting concerns (e.g., logging, validation, metrics) |

The abstractions for these message types are provided by [Archityped.Mediation.Abstractions](https://www.nuget.org/packages/Archityped.Mediation.Abstractions).  

This package provides the **runtime implementations** to execute them.

---

## 📚 Learn More

- Full documentation and usage guides: [github.com/Archityped/Mediation](https://github.com/Archityped/Mediation)
- Abstractions: [Archityped.Mediation.Abstractions](https://www.nuget.org/packages/Archityped.Mediation.Abstractions)

---

© [Archityped](https://github.com/Archityped) _– Building types to perfection_
