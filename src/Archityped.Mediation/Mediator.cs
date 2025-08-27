namespace Archityped.Mediation;

/// <summary>
/// Represents a mediator that can send requests and publish notifications.
/// </summary>
public sealed class Mediator : MediatorBase, IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INotificationPublisher _notificationPublisher;

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve handlers and middleware.</param>
    /// <returns>A new instance of the <see cref="Mediator"/> class configured with the specified dependencies.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _notificationPublisher = serviceProvider.GetService<INotificationPublisher>() ?? new NotificationPublisher(serviceProvider);
    }

    /// <inheritdoc/>
    protected override INotificationPublisher GetNotificationPublisher()
        => _notificationPublisher;

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">No handler is registered for the specified request type.</exception>
    protected override IRequestHandler<TRequest> GetRequestHandler<TRequest>()
        => _serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">No handler is registered for the specified request type.</exception>
    protected override IRequestHandler<TRequest, TResponse> GetRequestHandler<TRequest, TResponse>()
        => _serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">No handler is registered for the specified stream request type.</exception>
    protected override IStreamRequestHandler<TRequest, TResponse> GetStreamRequestHandler<TRequest, TResponse>()
        => _serviceProvider.GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>();

    /// <inheritdoc/>
    protected override IReadOnlyList<IRequestMiddleware> GetRequestMiddleware()
        => Unsafe.As<IReadOnlyList<IRequestMiddleware>>(_serviceProvider.GetServices<IRequestMiddleware>());

    /// <inheritdoc/>
    protected override IReadOnlyList<IStreamRequestMiddleware> GetStreamRequestMiddleware()
        => Unsafe.As<IReadOnlyList<IStreamRequestMiddleware>>(_serviceProvider.GetServices<IStreamRequestMiddleware>());
}
