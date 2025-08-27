namespace Archityped.Mediation;

/// <summary>
/// Represents an in-process notification publisher that distributes notifications to registered handlers concurrently.
/// </summary>
public class NotificationPublisher : INotificationPublisher
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationPublisher"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve notification handlers.</param>
    /// <returns>A new instance of the <see cref="NotificationPublisher"/> class configured with the specified service provider.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
    public NotificationPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc/>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public virtual async Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        cancellationToken.ThrowIfCancellationRequested();
        var handlers = GetNotificationHandlers<TNotification>();
        await Task.WhenAll(handlers.Select(handler => handler.HandleAsync(notification, cancellationToken)));
    }

    /// <summary>
    /// Gets the collection of notification handlers for the specified notification type.
    /// </summary>
    /// <typeparam name="TNotification">The type of notification to get handlers for.</typeparam>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="INotificationHandler{TNotification}"/> instances that can handle the specified notification type.</returns>
    protected virtual IReadOnlyList<INotificationHandler<TNotification>> GetNotificationHandlers<TNotification>() where TNotification : INotification
        => Unsafe.As<IReadOnlyList<INotificationHandler<TNotification>>>(_serviceProvider.GetServices<INotificationHandler<TNotification>>());
}
