namespace Archityped.Mediation;

// Due to AOT limitations in the service registry, generic types for this interface are not currently supported.
// We plan to revisit and potentially add support in future versions.

/// <summary>
/// Represents the method that handles the execution of the next middleware or handler in the request pipeline.
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
/// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation and contains the response of type <typeparamref name="TResponse"/>.</returns>
/// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken);

/// <summary>
/// Represents middleware that can intercept and process requests in the mediation pipeline.
/// </summary>
/// <remarks>
/// Implementations can perform cross-cutting concerns such as logging, validation, caching, or authorization
/// before and after the request is processed by subsequent middleware or handlers.
/// </remarks>
public interface IRequestMiddleware
{
    /// <summary>
    /// Asynchronously processes a request through the middleware pipeline.
    /// </summary>
    /// <typeparam name="TRequest">The type of request that implements <see cref="IBaseRequest"/>.</typeparam>
    /// <typeparam name="TResponse">The type of response returned by the handler.</typeparam>
    /// <param name="request">The request to process.</param>
    /// <param name="next">The delegate representing the next middleware or handler in the pipeline.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the asynchronous operation and contains the response of type <typeparamref name="TResponse"/>.
    /// </returns>
    Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
        where TRequest : IBaseRequest;
}
