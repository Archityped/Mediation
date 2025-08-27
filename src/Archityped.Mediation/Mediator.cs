using Microsoft.Extensions.DependencyInjection;

namespace Archityped.Mediation;

/// <summary>
/// Represents a dependency injection-based mediator implementation that resolves handlers and middleware from an <see cref="IServiceProvider"/>.
/// </summary>
/// <remarks>
/// This implementation uses the Microsoft.Extensions.DependencyInjection framework to resolve event handlers, request handlers, and middleware components.
/// All handlers and middleware must be registered in the service container before being resolved by this mediator.
/// </remarks>
public sealed class Mediator : BaseMediator
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class with the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve handlers and middleware components.</param>
    /// <returns>A new instance of the <see cref="Mediator"/> class configured to use the specified service provider for dependency resolution.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    protected override IEnumerable<IEventHandler<TEvent>> GetEventHandlers<TEvent>()
        => _serviceProvider.GetServices<IEventHandler<TEvent>>();

    /// <inheritdoc/>
    protected override IRequestHandler<TRequest> GetRequestHandler<TRequest>()
        => _serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();

    /// <inheritdoc/>
    protected override IRequestHandler<TRequest, TResponse> GetRequestHandler<TRequest, TResponse>()
        => _serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

    /// <inheritdoc/>
    protected override IEnumerable<IRequestMiddleware> GetRequestMiddleware()
        => _serviceProvider.GetServices<IRequestMiddleware>();

    /// <inheritdoc/>
    protected override IStreamRequestHandler<TRequest, TResponse> GetStreamRequestHandler<TRequest, TResponse>()
        => _serviceProvider.GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>();

    /// <inheritdoc/>
    protected override IEnumerable<IStreamRequestMiddleware> GetStreamRequestMiddleware()
        => _serviceProvider.GetServices<IStreamRequestMiddleware>();
}
