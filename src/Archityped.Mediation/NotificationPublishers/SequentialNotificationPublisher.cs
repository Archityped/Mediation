namespace Archityped.Mediation.NotificationPublishers;

/// <summary>
/// Represents an in-process notification publisher that distributes notifications to registered handlers sequentially in order.
/// </summary>
/// <remarks>
/// This implementation processes notification handlers one at a time in the order they are resolved, 
/// rather than concurrently. Each handler must complete before the next handler is invoked.
/// </remarks>
public class SequentialNotificationPublisher : NotificationPublisher
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SequentialNotificationPublisher"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve notification handlers.</param>
    /// <returns>A new instance of the <see cref="SequentialNotificationPublisher"/> class configured with the specified service provider.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
    public SequentialNotificationPublisher(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <inheritdoc/>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public override async Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
    {
        var handlers = GetNotificationHandlers<TNotification>();

        for (int index = 0; index < handlers.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var handler = handlers[index];
            await handler.HandleAsync(notification, cancellationToken).ConfigureAwait(false);
        }
    }
}
