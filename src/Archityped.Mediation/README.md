# Archityped.Mediation

**Archityped.Mediation** is a modern, high-performance, and source-generator‑powered mediation library for .NET. It unifies in-process messaging, CQRS, and event-driven patterns under a clean abstraction—designed to be composable, extensible, and NativeAOT-friendly.

This package provides the **runtime implementation**, including handler resolution, middleware pipelines, and integration with source-generated dispatch extensions.

---

## 🧩 Key Features

- **Request/Response Handling**  
  Strongly typed request/response workflows using `IRequest<TResponse>` and `IRequestHandler<TRequest, TResponse>`.

- **Event Publishing**  
  Dispatch domain events to multiple handlers using `IEvent` and `IEventHandler<TEvent>`.

- **Streaming Support**  
  Stream responses with `IStreamRequest<TResponse>` and `IStreamRequestHandler<TRequest, TResponse>`—ideal for batched, paged, or progressive results.

- **Middleware Pipelines**  
  Plug in cross-cutting behaviors (logging, metrics, caching, validation, etc.) via `IRequestMiddleware` and `IStreamRequestMiddleware`.

- **Source-Generated Dispatching**  
  Avoid runtime reflection. The optional [Archityped.Mediation.SourceGenerator](https://www.nuget.org/packages/Archityped.Mediation.SourceGenerator) enables fast, compile-time wiring of `SendAsync`, `PublishAsync`, and `StreamAsync` calls.

- **Dependency Injection Friendly**  
  Built for `Microsoft.Extensions.DependencyInjection`. Supports modular registration with assembly scanning and configuration hooks.

---

## 🚀 Getting Started

A minimal example:

```csharp
// Register the mediator and scan for handlers
services.AddMediator(typeof(Program).Assembly);

// Define a request/response
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

## 📚 Learn More

- Full documentation & usage guides: [github.com/Archityped/Mediation](https://github.com/Archityped/Mediation)
- Explore the [Archityped.Mediation.Abstractions](https://www.nuget.org/packages/Archityped.Mediation.Abstractions) package for core interfaces

---

© [Archityped](https://github.com/Archityped) _– Building types to perfection_
