# Archityped.Mediation.SourceGenerator

**Archityped.Mediation.SourceGenerator** is a Roslyn-based source generator for the Archityped.Mediation ecosystem. It eliminates the need for runtime reflection when wiring up requests, handlers, and middleware, enabling **high-performance resolution and NativeAOT compatibility**.

This package is typically referenced alongside `Archityped.Mediation` to provide compile-time optimizations.

---

## Installation

> IMPORTANT  
> This package is **not intended for direct installation**. It is consumed automatically when you reference [Archityped.Mediation.Abstractions](https://www.nuget.org/packages/Archityped.Mediation.Abstractions).  
>
> You should not add a direct dependency on this package in your projects.

---

## Key Features

- **Type Inference Helpers**  
  Generates extension methods so you can call `SendAsync(request)` or `StreamAsync(request)` without explicitly specifying generic type parameters.  

- **Performance Optimized**  
  Removes runtime reflection when dispatching requests and streams, resulting in faster execution and safer NativeAOT scenarios.  

---

## Usage

When included in your project, the source generator runs at compile time and augments the mediator with generated resolvers. No additional configuration is required.

Example registration with explicit handlers:

```csharp
services.AddMediator(cfg => cfg
    .AddRequestHandler<CreateUserHandler>()
    .AddEventHandler<UserRegisteredHandler>());
```

---

## Documentation

For full documentation, examples, and guidance, visit [Archityped.Mediation](https://github.com/Archityped/Mediation).  