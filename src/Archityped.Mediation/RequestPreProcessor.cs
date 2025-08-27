namespace Archityped.Mediation;

/// <summary>
/// Represents request middleware that invokes all registered <see cref="IRequestPreProcessor"/> instances
/// before delegating execution to the next component in the pipeline.
/// </summary>
/// <param name="preProcessors">
/// The ordered collection of pre-processors to execute. Each pre-processor is awaited sequentially prior to invoking
/// the next middleware or handler.
/// </param>
internal readonly struct RequestPreProcessor(IEnumerable<IRequestPreProcessor> preProcessors) : IRequestMiddleware
{
    /// <inheritdoc/>
    public async Task<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken = default)
        where TRequest : IBaseRequest
    {
        foreach (var processor in preProcessors)
        {
            await processor.ProcessAsync<TRequest, TResponse>(request, cancellationToken).ConfigureAwait(false);
        }

        return await next(cancellationToken).ConfigureAwait(false);
    }
}
