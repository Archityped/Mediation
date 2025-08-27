namespace Archityped.Mediation;

// Due to AOT limitations in the service registry, generic types for this interface are not currently supported.
// We plan to revisit and potentially add support in future versions.

/// <summary>
/// Represents the method that handles the next stage in a streaming request middleware pipeline.
/// </summary>
/// <typeparam name="TResponse">The type of the response elements in the stream.</typeparam>
/// <param name="cancellationToken">
/// A token to monitor for cancellation requests. If canceled, the operation is terminated and an <see cref="OperationCanceledException"/> is thrown.
/// </param>
/// <returns>
/// An <see cref="IAsyncEnumerable{T}"/> that represents the asynchronous stream of <typeparamref name="TResponse"/> responses.
/// </returns>
/// <exception cref="OperationCanceledException">Thrown if the operation is canceled via <paramref name="cancellationToken"/>.</exception>
public delegate IAsyncEnumerable<TResponse> StreamRequestHandlerDelegate<TResponse>(CancellationToken cancellationToken);

/// <summary>
/// Represents middleware that can process streaming requests in a mediator pipeline.
/// </summary>
public interface IStreamRequestMiddleware
{
    /// <summary>
    /// Asynchronously processes a streaming request through the middleware pipeline.
    /// </summary>
    /// <typeparam name="TRequest">The type of the streaming request. Must implement <see cref="IStreamRequest{TResponse}"/>.</typeparam>
    /// <typeparam name="TResponse">The type of the response returned by the handler.</typeparam>
    /// <param name="request">The streaming request to process. Must not be <see langword="null"/>.</param>
    /// <param name="next">The delegate representing the next middleware or handler in the pipeline.</param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests. If canceled, the operation is terminated and an <see cref="OperationCanceledException"/> is thrown.
    /// </param>
    /// <returns>
    /// An <see cref="IAsyncEnumerable{T}"/> that represents the asynchronous stream of <typeparamref name="TResponse"/> responses.
    /// </returns>
    IAsyncEnumerable<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, StreamRequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
        where TRequest : IStreamRequest<TResponse>;
}