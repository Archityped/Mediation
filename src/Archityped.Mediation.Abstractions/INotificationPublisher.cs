namespace Archityped.Mediation;

/// <summary>
/// Represents an object that can publish notifications.
/// </summary>
public interface INotificationPublisher
{
    /// <summary>
    /// Asynchronously publishes the specified notification to all registered handlers.
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification to publish. Must implement <see cref="INotification"/>.</typeparam>
    /// <param name="notification">The notification instance to publish.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous publish operation.</returns>
    Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification;
}