# Archityped.Mediation

**Archityped.Mediation** is a modern, extensible, and high-performance mediation library for .NET. It provides a unified abstraction for in-process messaging, CQRS patterns, and event-driven architectures. With support for request/response, event publishing, and streaming scenarios, Archityped.Mediation enables developers to build clean, maintainable, and decoupled applications.

This package is the **full runtime implementation**, including handler resolution, middleware pipelines, and source-generated optimizations.

---

## Installation

Install via NuGet:

```shell
dotnet add package Archityped.Mediation
```

---

## Key Features

- **Request/Response**  
  Strongly typed request/response handling with `IRequest<TResponse>` and `IRequestHandler<TRequest,TResponse>`.  

- **Event Publishing**  
  Publish events to multiple handlers with `IEvent` and `IEventHandler<TEvent>`.  

- **Streaming Requests**  
  Stream results using `IStreamRequest<TResponse>` and `IStreamRequestHandler<TRequest,TResponse>`.  

- **Middleware Pipelines**  
  Add cross-cutting concerns such as logging, validation, metrics, or caching using `IRequestMiddleware` and `IStreamRequestMiddleware`.  

- **Source-Generated Type Inference**  
  Eliminates runtime reflection for request dispatching by generating extension methods for `SendAsync`, `PublishAsync`, and `StreamAsync`.  

- **Dependency Injection**  
  Seamless integration with `Microsoft.Extensions.DependencyInjection`.  

---

## Usage

A minimal example:

```csharp
// Register mediator and discover handlers from assembly
services.AddMediator(typeof(Program).Assembly);

// Define a request and response
public record Ping(string Message) : IRequest<Pong>;
public record Pong(string Response);

// Implement the handler
public class PingHandler : IRequestHandler<Ping, Pong>
{
    public Task<Pong> HandleAsync(Ping request, CancellationToken ct = default) =>
        Task.FromResult(new Pong($"Pong: {request.Message}"));
}

// Use the mediator
var mediator = provider.GetRequiredService<IMediator>();
var result = await mediator.SendAsync(new Ping("Hello World"));
```

---

## Documentation

For full documentation, examples, and guidance, visit [Archityped.Mediation](https://github.com/Archityped/Mediation).  
