namespace Archityped.Mediation;

/// <summary>
/// Represents an object that can publish events to subscribers.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Asynchronously publishes the specified event to all registered subscribers.
    /// </summary>
    /// <typeparam name="TEvent">The type of the event to publish. Must implement <see cref="IEvent"/>.</typeparam>
    /// <param name="event">The event instance to publish.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous publish operation.</returns>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;
}