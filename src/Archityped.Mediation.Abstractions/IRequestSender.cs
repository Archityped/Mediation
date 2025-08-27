namespace Archityped.Mediation;

/// <summary>
/// Represents an object capable of sending requests and receiving responses using the mediator pattern.
/// </summary>
public interface IRequestSender
{
    /// <summary>
    /// Asynchronously sends a request and returns a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to send. Must implement <see cref="IRequest{TResponse}"/>.</typeparam>
    /// <typeparam name="TResponse">The type of the response returned by the request.</typeparam>
    /// <param name="request">The request to send. Must not be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. If canceled, the operation is terminated and an <see cref="OperationCanceledException"/> is thrown.</param>
    /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation. The task result contains the response.</returns>
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest<TResponse>;

    /// <summary>
    /// Asynchronously sends a request that does not return a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to send. Must implement <see cref="IRequest"/>.</typeparam>
    /// <param name="request">The request to send. Must not be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. If canceled, the operation is terminated and an <see cref="OperationCanceledException"/> is thrown.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest;

    /// <summary>
    /// Asynchronously sends a stream request and returns an asynchronous stream of responses.
    /// </summary>
    /// <typeparam name="TRequest">The type of the stream request to send. Must implement <see cref="IStreamRequest{TResponse}"/>.</typeparam>
    /// <typeparam name="TResponse">The type of the response elements in the stream.</typeparam>
    /// <param name="request">The stream request to send. Must not be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. If canceled, the operation is terminated and an <see cref="OperationCanceledException"/> is thrown.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that represents the asynchronous stream of responses.</returns>
    IAsyncEnumerable<TResponse> StreamAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IStreamRequest<TResponse>;
}