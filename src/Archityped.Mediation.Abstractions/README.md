# Archityped.Mediation.Abstractions

**Archityped.Mediation.Abstractions** defines the contracts that make up the Archityped.Mediation ecosystem.  

It is designed for scenarios where you want to **model mediator interactions in shared or domain libraries** without introducing a dependency on the runtime implementation. By depending only on abstractions, you can write code against a stable contract and keep infrastructure concerns isolated.  

---

## Installation

```shell
dotnet add package Archityped.Mediation.Abstractions
```

> TIP 
> Most application projects should reference **[Archityped.Mediation](https://www.nuget.org/packages/Archityped.Mediation)**.  
> Use **Archityped.Mediation.Abstractions** when building shared or domain libraries that should not depend on the runtime.

---

## Key Features

- **Mediator Abstraction**  
  `IMediator` - Unified entry point for sending requests, publishing events, and streaming responses  

- **Request/Response**  
  - `IRequest` - Marker for requests that do not return a value  
  - `IRequestHandler<TRequest>` - Handles `IRequest`

  - `IRequest<TResponse>` - Represents a request expecting a typed response  
  - `IRequestHandler<TRequest,TResponse>` - Handles `IRequest<TResponse>`  

- **Event Publishing**  
  - `IEvent` - Marker for events  
  - `IEventHandler<TEvent>` - Handles published events  
  - `IEventPublisher` - Abstraction for broadcasting events  

- **Streaming Requests**  
  - `IStreamRequest<TResponse>` - Represents a request that return a stream of responses  
  - `IStreamRequestHandler<TRequest,TResponse>` - Handles streaming requests  

- **Middleware Pipelines**  
  - `IRequestMiddleware` - Pipeline behaviors for request/response  
  - `IStreamRequestMiddleware` - Pipeline behaviors for streaming requests  
  - `IRequestSender` - Abstraction for dispatching requests through the pipeline  

---

## Example

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

At runtime, **Archityped.Mediation** provides the concrete implementations for resolution, pipelines, and dispatch.  

---

## Documentation

For full documentation, examples, and guidance, visit [Archityped.Mediation](https://github.com/Archityped/Mediation).  