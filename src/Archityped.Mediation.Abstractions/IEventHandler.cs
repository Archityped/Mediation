namespace Archityped.Mediation;

/// <summary>
/// Represents a handler for an event.
/// </summary>
/// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
public interface IEventHandler<in TEvent>: IBaseEventHandler where TEvent : IEvent
{
    /// <summary>
    /// Asynchronously handles the specified event.
    /// </summary>
    /// <param name="event">The event instance to handle.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
