namespace Archityped.Mediation;

/// <summary>
/// Represents a component that executes logic after a request has been handled in the mediation pipeline.
/// </summary>
public interface IRequestPostProcessor: IBaseRequestProcessor
{
    /// <summary>
    /// Asynchronously performs post-processing logic for the specified request after it has been handled.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request that was processed.</typeparam>
    /// <typeparam name="TResponse">The type of the response produced by the request handler.</typeparam>
    /// <param name="request">The request instance that was handled.</param>
    /// <param name="response">The response produced by the handler, or <see langword="null"/> if the handler returned <see langword="null"/> or no value.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous post-processing operation.</returns>
    ValueTask ProcessAsync<TRequest, TResponse>(TRequest request, TResponse? response, CancellationToken cancellationToken = default)
        where TRequest : IBaseRequest;
}
