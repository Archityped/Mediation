namespace Archityped.Mediation;

/// <summary>
/// Represents request middleware that invokes all registered <see cref="IRequestPostProcessor"/> instances
/// after the request handler has produced a response, allowing post-processing concerns to be applied.
/// </summary>
/// <param name="postProcessors">
/// The ordered collection of post-processors to execute. Each post-processor is awaited sequentially after the handler
/// completes and before the final response is returned to the caller.
/// </param>
internal readonly struct RequestPostProcessor(IEnumerable<IRequestPostProcessor> postProcessors) : IRequestMiddleware
{
    /// <inheritdoc/>
    public async Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
        where TRequest : IBaseRequest
    {
        var response = await next(cancellationToken).ConfigureAwait(false);

        foreach (var processor in postProcessors)
        {
            await processor.ProcessAsync(request, response, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }
}