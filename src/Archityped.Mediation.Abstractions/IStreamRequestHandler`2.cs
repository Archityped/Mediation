namespace Archityped.Mediation;

/// <summary>
/// Represents a handler for stream requests that produces an asynchronous stream of responses.
/// </summary>
/// <typeparam name="TRequest">The type of the stream request message. Must implement <see cref="IStreamRequest{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The type of the response elements produced by the handler.</typeparam>
public interface IStreamRequestHandler<TRequest, TResponse> : IBaseStreamRequestHandler
     where TRequest : IStreamRequest<TResponse>
{
    /// <summary>
    /// Asynchronously handles a stream request and returns an asynchronous stream of responses.
    /// </summary>
    /// <param name="request">The stream request message to handle. Must not be <see langword="null"/>.</param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests. If canceled, the operation is terminated and an <see cref="OperationCanceledException"/> is thrown.
    /// </param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that represents the asynchronous stream of <typeparamref name="TResponse"/> elements.</returns>
    IAsyncEnumerable<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}