# Archityped.Mediation

**Archityped.Mediation** is a modern, extensible, and high-performance .NET mediation library for in-process messaging, CQRS, and event-driven architectures. It provides a robust abstraction for request/response, streaming, and event publishing patterns, with first-class support for middleware, dependency injection, and source-generated optimizations.

This repository contains two packages:  
- **Archityped.Mediation** : The full implementation, including registration, middleware, and runtime execution.  
- **Archityped.Mediation.Abstractions** : A lightweight package with only the contracts (`IRequest<TResponse>`, `IEvent`, handlers, etc.). Use this when defining requests and events in shared libraries without depending on the runtime.  

---

## Key Features

- **Request/Response**  
  Strongly-typed request and response handling via `IRequest<TResponse>` and `IRequestHandler<TRequest, TResponse>`.

- **Event Publishing**  
  Publish events to multiple handlers using `IEvent` and `IEventHandler<TEvent>` for decoupled, event-driven systems.

- **Streaming Requests**  
  Process asynchronous streams of data with `IStreamRequest<TResponse>` and `IStreamRequestHandler<TRequest, TResponse>`.

- **Middleware Pipelines**  
  Add cross-cutting behaviors such as logging, validation, or metrics with `IRequestMiddleware` and `IStreamRequestMiddleware`.

- **Source-Generated Type Inference**  
  High-performance request resolution with compile-time generated helpers. No runtime reflection is required (other than dependency injection).

- **Dependency Injection**  
  First-class integration with `Microsoft.Extensions.DependencyInjection`.

- **Broad Compatibility**  
  Supports both .NET Standard 2.0 and .NET 8, ensuring it runs in modern and legacy applications.

---

## Installation

Install via NuGet:

```shell
# .NET CLI
dotnet add package Archityped.Mediation
```

For contracts only (requests, events, handlers):

```shell
dotnet add package Archityped.Mediation.Abstractions
```

Use **Archityped.Mediation.Abstractions** in shared libraries to avoid bringing in runtime dependencies.

---

## Quickstart

A minimal end-to-end example:

```csharp
// 1. Register mediator
services.AddMediator(typeof(Program).Assembly);

// 2. Define request and handler
public record Ping(string Message) : IRequest<Pong>;
public record Pong(string Response);

public class PingHandler : IRequestHandler<Ping, Pong>
{
    public Task<Pong> HandleAsync(Ping request, CancellationToken cancellationToken = default) =>
        Task.FromResult(new Pong($"Pong: {request.Message}"));
}

// 3. Send request
var mediator = provider.GetRequiredService<IMediator>();
var result = await mediator.SendAsync(new Ping("Hello"));

Console.WriteLine(result.Response); // -> Pong: Hello
```

---

## Getting Started

### 1. Register the Mediator

```csharp
// Scan assembly for handlers
services.AddMediator(typeof(Program).Assembly);

// Or register explicitly (AOT safe)
services.AddMediator(cfg => cfg
    .AddRequestHandler<CreateOrderHandler>()
    .AddEventHandler<OrderCreatedHandler>()
    .AddStreamRequestHandler<ExportOrdersHandler>()
    .AddRequestMiddleware<RequestLoggingMiddleware>()
    .AddStreamRequestMiddleware<StreamMetricsMiddleware>());
```

For trimming or NativeAOT scenarios, prefer **explicit registration** to ensure all handlers are available at runtime.

---

### 2. Usage Examples

#### 2.1 Request/Response (persisting with repository/DbContext)

```csharp
public record CreateOrder(string CustomerEmail, decimal Total) : IRequest<OrderResult>;
public record OrderResult(Guid OrderId, bool Success);

public interface IOrderRepository
{
    Task<Guid> SaveAsync(string email, decimal total, CancellationToken ct);
}

public class CreateOrderHandler(IOrderRepository repo, ILogger<CreateOrderHandler> logger)
    : IRequestHandler<CreateOrder, OrderResult>
{
    public async Task<OrderResult> HandleAsync(CreateOrder request, CancellationToken ct = default)
    {
        var orderId = await repo.SaveAsync(request.CustomerEmail, request.Total, ct);
        logger.LogInformation("Created order {OrderId} for {Email}", orderId, request.CustomerEmail);
        return new OrderResult(orderId, Success: true);
    }
}

var result = await mediator.SendAsync(new CreateOrder("buyer@example.com", 199.99m));
```

#### 2.2 Events (publishing to a message bus or sending an email)

```csharp
public record OrderCreated(Guid OrderId, string CustomerEmail) : IEvent;

public interface IMessageBus
{
    Task PublishAsync(string topic, object payload, CancellationToken ct);
}

public class OrderCreatedHandler(IMessageBus bus, ILogger<OrderCreatedHandler> logger)
    : IEventHandler<OrderCreated>
{
    public async Task HandleAsync(OrderCreated @event, CancellationToken ct = default)
    {
        await bus.PublishAsync("orders.created", @event, ct);
        logger.LogInformation("Published OrderCreated for {OrderId}", @event.OrderId);
    }
}

await mediator.PublishAsync(new OrderCreated(Guid.NewGuid(), "buyer@example.com"));
```

#### 2.3 Streaming Requests (fetching async from repository/storage)

```csharp
public record ExportOrders() : IStreamRequest<string>;
public record OrderRow(Guid Id, string Email);

public interface IOrderReadRepository
{
    IAsyncEnumerable<OrderRow> GetAllAsync(CancellationToken ct);
}

public class ExportOrdersHandler(IOrderReadRepository repo)
    : IStreamRequestHandler<ExportOrders, string>
{
    public async IAsyncEnumerable<string> HandleAsync(
        ExportOrders request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        yield return "OrderId,CustomerEmail"; // Header

        await foreach (var row in repo.GetAllAsync(ct))
        {
            yield return $"{row.Id},{row.Email}";
        }
    }
}

await foreach (var line in mediator.StreamAsync(new ExportOrders()))
{
    Console.WriteLine(line);
}
```

#### 2.4 Middleware Examples

##### 2.4.1 IRequestMiddleware (logging & exception capture)

```csharp
public class RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger) : IRequestMiddleware
{
    public async Task<TResponse> InvokeAsync<TRequest, TResponse>(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct = default)
        where TRequest : IBaseRequest
    {
        try
        {
            logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);
            var response = await next(ct);
            logger.LogInformation("Handled {RequestName}", typeof(TRequest).Name);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}
```

##### 2.4.2 IStreamRequestMiddleware (collecting metrics with start/increment/stop)

```csharp
public interface IMetricsCollector
{
    Task StartAsync(string name, CancellationToken ct = default);
    Task IncrementAsync(string name, CancellationToken ct = default);
    Task StopAsync(string name, int itemCount, TimeSpan duration, CancellationToken ct = default);
}

public class StreamMetricsMiddleware(IMetricsCollector metrics, ILogger<StreamMetricsMiddleware> logger)
    : IStreamRequestMiddleware
{
    public async IAsyncEnumerable<TResponse> InvokeAsync<TRequest, TResponse>(
        TRequest request,
        StreamRequestHandlerDelegate<TResponse> next,
        [EnumeratorCancellation] CancellationToken ct = default)
        where TRequest : IBaseStreamRequest
    {
        var streamName = typeof(TRequest).Name;
        var count = 0;
        var sw = Stopwatch.StartNew();

        await metrics.StartAsync(streamName, ct);

        try
        {
            await foreach (var item in next(ct))
            {
                count++;
                await metrics.IncrementAsync(streamName, ct);
                yield return item;
            }

            sw.Stop();
            await metrics.StopAsync(streamName, count, sw.Elapsed, ct);

            logger.LogInformation("[Stream] {StreamName}: {Count} items in {ElapsedMs} ms",
                streamName, count, sw.Elapsed.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Stream] {StreamName} failed after {Count} items", streamName, count);
            throw;
        }
    }
}
```

---

## API Overview

### Contracts

- `IRequest<TResponse>` | `IRequestHandler<TRequest, TResponse>` : Request/response pattern.  
- `IEvent` | `IEventHandler<TEvent>` : Event publishing.  
- `IStreamRequest<TResponse>` | `IStreamRequestHandler<TRequest, TResponse>` : Streaming requests.  
- `IRequestMiddleware` | `IStreamRequestMiddleware` : Middleware pipelines.  
- **Archityped.Mediation.Abstractions** : Contracts-only package for defining requests, events, and handlers.  

### Runtime Components

- `IMediator` : Main entry point for sending requests, publishing events, and streaming results.  
- `ServiceCollectionExtensions.AddMediator(...)` : Extension methods for registration and dependency injection.  
- Source-generated resolvers : Enables high-performance type inference and avoids runtime reflection.  

---

## Contributing

We welcome contributions!  
To report issues, request features, or submit pull requests, please visit the [GitHub repository](./).

---

## License

This project is licensed under the **Apache License 2.0**.  
See the [LICENSE](LICENSE) file for details.

---

> ## Acknowledgements  
> Archityped.Mediation is inspired by [MediatR](https://github.com/jbogard/MediatR) by Jimmy Bogard.  
> MediatR is a mature and widely used library — if it fits your needs, please consider supporting its development through [GitHub Sponsors](https://github.com/sponsors/jbogard).  
>  
> Archityped.Mediation builds on those ideas, with a focus on source-generated performance, high-throughput pipelines, and AOT-safe registration for modern .NET runtimes.
