namespace Archityped.Mediation;

/// <summary>
/// Represents a component that executes logic before a request is handled in the mediation pipeline.
/// </summary>
public interface IRequestPreProcessor : IBaseRequestProcessor
{
    /// <summary>
    /// Asynchronously performs pre-processing logic for the specified request before it is handled.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request being processed.</typeparam>
    /// <typeparam name="TResponse">The type of the response expected from the request handler.</typeparam>
    /// <param name="request">The request instance to pre-process.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous pre-processing operation.</returns>
    ValueTask ProcessAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IBaseRequest;
}
