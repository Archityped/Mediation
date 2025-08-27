namespace Archityped.Mediation;

/// <summary>
/// Represents a handler for a notification.
/// </summary>
/// <typeparam name="TNotification">The type of notification to subscribe to.</typeparam>
public interface INotificationHandler<in TNotification> : IBaseNotificationHandler where TNotification : INotification
{
    /// <summary>
    /// Asynchronously handles the specified notification.
    /// </summary>
    /// <param name="notification">The notification instance to handle.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken = default);
}
