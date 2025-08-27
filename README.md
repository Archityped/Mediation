# Archityped.Mediation

[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

**Archityped.Mediation** is a high-performance mediation library for .NET.  
It provides a unified abstraction for in-process messaging, CQRS, and event-driven patterns, supporting request/response, notifications, and streaming messages with built-in middleware and dependency injection support.

This repository contains two packages:

| Package | Description |
|----------|-------------|
| [**Archityped.Mediation.Abstractions**](./src/Archityped.Mediation.Abstractions) <br><br> [![NuGet](https://img.shields.io/nuget/v/Archityped.Mediation.Abstractions.svg?label=NuGet)](https://www.nuget.org/packages/Archityped.Mediation.Abstractions) [![Downloads](https://img.shields.io/nuget/dt/Archityped.Mediation.Abstractions.svg?label=downloads)](https://www.nuget.org/packages/Archityped.Mediation.Abstractions) | Core interfaces and contracts such as `IRequest`, `INotification`, `IStreamRequest`, and `IMediator`. Use in shared or domain libraries to define messages and handlers without introducing runtime dependencies. |
| [**Archityped.Mediation**](./src/Archityped.Mediation) <br><br> [![NuGet](https://img.shields.io/nuget/v/Archityped.Mediation.svg?label=NuGet)](https://www.nuget.org/packages/Archityped.Mediation) [![Downloads](https://img.shields.io/nuget/dt/Archityped.Mediation.svg?label=downloads)](https://www.nuget.org/packages/Archityped.Mediation) | Runtime implementation providing handler resolution, middleware pipelines, and message dispatching. Use in application projects to register handlers, configure middleware, and execute messages. |

---

## Key Features

- **Request/Response Handling** – Strongly-typed requests and responses via `IRequest<TResponse>` and `IRequestHandler<TRequest, TResponse>`.  
- **Notification Publishing** – Publish notifications to multiple handlers for decoupled message processing.  
- **Streaming Support** – Support for `IStreamRequest<TResponse>` and progressive data handling.  
- **Middleware Pipelines** – Apply cross-cutting concerns such as logging, validation, and metrics.  
- **Source-Generated Type Inference** – Compile-time message resolution without runtime reflection.  
- **Dependency Injection Integration** – Works seamlessly with `Microsoft.Extensions.DependencyInjection`.  

---

## Installation

Install from NuGet:

```shell
# Runtime implementation
dotnet add package Archityped.Mediation
```

For abstractions only:

```shell
dotnet add package Archityped.Mediation.Abstractions
```

---

## Example

A minimal end-to-end example:

```csharp
// Define a request and response
public record Ping(string Message) : IRequest<Pong>;
public record Pong(string Response);

// Implement a handler
public class PingHandler : IRequestHandler<Ping, Pong>
{
    public Task<Pong> HandleAsync(Ping request, CancellationToken ct = default) =>
        Task.FromResult(new Pong($"Pong: {request.Message}"));
}

// Configure dependency injection
var services = new ServiceCollection();
services.AddMediator(cfg =>
{
    cfg.AddRequestHandler<PingHandler>();
});

// Build the provider
using var provider = services.BuildServiceProvider();

// Resolve and use IMediator
var mediator = provider.GetRequiredService<IMediator>();
var response = await mediator.SendAsync(new Ping("Hello"));

Console.WriteLine(response.Response); // Output: Pong: Hello
```

---

## Contributing

We welcome contributions.  
Please open an issue or submit a pull request to report bugs, request features, or propose improvements.

---

## License

This project is licensed under the **Apache License 2.0**.  
See the [LICENSE](LICENSE) file for details.

---

&nbsp;

## Acknowledgements  
Archityped.Mediation is inspired by [MediatR](https://github.com/jbogard/MediatR) by Jimmy Bogard.  
MediatR is a mature and widely used library; if it fits your needs, please consider supporting its development through [GitHub Sponsors](https://github.com/sponsors/jbogard).  
  
Archityped.Mediation builds on those ideas, with a focus on source-generated performance, high-throughput pipelines, and AOT-safe registration for modern .NET runtimes.
