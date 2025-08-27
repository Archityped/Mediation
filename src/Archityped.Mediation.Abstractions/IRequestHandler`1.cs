namespace Archityped.Mediation;

/// <summary>
/// Represents a handler for a request that does not return a response.
/// </summary>
/// <typeparam name="TRequest">The type of request to handle.</typeparam>
public interface IRequestHandler<in TRequest> : IBaseRequestHandler
    where TRequest : IRequest
{
    /// <summary>
    /// Asynchronously handles the specified request.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
