# Archityped.Mediation.Abstractions

**Archityped.Mediation.Abstractions** defines the core interfaces and message contracts for the [Archityped.Mediation](https://www.nuget.org/packages/Archityped.Mediation) ecosystem.

This package is designed for scenarios where you want to model mediator interactions within **shared**, **domain**, or **decoupled** projects—without depending on the runtime implementation. It enables clean boundaries, testable components, and layered architectures.

---

## 🧩 Key Features

- **Mediator Abstraction**  
  - `IMediator` – Unified entry point for sending requests, publishing events, and streaming responses  
  - `IRequestSender` – Dispatch abstraction that flows through middleware pipelines  

- **Request/Response Contracts**  
  - `IRequest<TResponse>` – Marker for requests expecting a typed result  
  - `IRequestHandler<TRequest, TResponse>` – Handles typed requests  
  - `IRequest` – Marker for void-returning requests  
  - `IRequestHandler<TRequest>` – Handles fire-and-forget requests  

- **Event Publishing**  
  - `IEvent` – Marker for events  
  - `IEventHandler<TEvent>` – Handles published events  
  - `IEventPublisher` – Abstraction for dispatching events to multiple handlers  

- **Streaming Requests**  
  - `IStreamRequest<TResponse>` – Represents a stream of responses  
  - `IStreamRequestHandler<TRequest, TResponse>` – Handles streamable requests  

- **Middleware Contracts**  
  - `IRequestMiddleware` – Middleware pipeline for request/response  
  - `IStreamRequestMiddleware` – Middleware pipeline for stream requests  

---

## 🚀 Getting Started

Use this package when building domain-level request/response definitions or reusable middleware.

Here’s a minimal domain-layer example:

```csharp
public record CreateUser(string Email) : IRequest<UserCreated>;
public record UserCreated(Guid Id, string Email);

public class CreateUserHandler : IRequestHandler<CreateUser, UserCreated>
{
    public Task<UserCreated> HandleAsync(CreateUser request, CancellationToken ct = default) =>
        Task.FromResult(new UserCreated(Guid.NewGuid(), request.Email));
}
```

This code depends only on abstractions.  

At runtime, the [Archityped.Mediation](https://www.nuget.org/packages/Archityped.Mediation) package provides handler resolution, dispatching, and middleware execution.

---

## 📚 Learn More

- Runtime implementation: [Archityped.Mediation](https://www.nuget.org/packages/Archityped.Mediation)
- Source generation support: [Archityped.Mediation.SourceGenerator](https://www.nuget.org/packages/Archityped.Mediation.SourceGenerator)

---

© [Archityped](https://github.com/Archityped) _– Building types to perfection_
