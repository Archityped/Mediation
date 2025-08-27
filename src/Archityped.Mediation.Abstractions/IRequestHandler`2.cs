namespace Archityped.Mediation;

/// <summary>
/// Represents a handler for a request that returns a response.
/// </summary>
/// <typeparam name="TRequest">The type of request to handle.</typeparam>
/// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
public interface IRequestHandler<in TRequest, TResponse> : IBaseRequestHandler
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Asynchronously handles the specified request and returns a response.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation, containing the response of type <typeparamref name="TResponse"/>.</returns>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}
