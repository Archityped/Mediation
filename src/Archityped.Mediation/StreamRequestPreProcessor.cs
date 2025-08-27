namespace Archityped.Mediation;

/// <summary>
/// Represents streaming request middleware that invokes all registered <see cref="IStreamRequestPreProcessor"/> instances
/// before delegating execution to the next component in the pipeline.
/// </summary>
/// <param name="preProcessors">
/// The ordered collection of pre-processors to execute. Each pre-processor is awaited sequentially prior to invoking
/// the next middleware or handler.
/// </param>
internal readonly struct StreamRequestPreProcessor(IEnumerable<IStreamRequestPreProcessor> preProcessors) : IStreamRequestMiddleware
{
    /// <inheritdoc/>
    public async IAsyncEnumerable<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, StreamRequestHandlerDelegate<TResponse> next, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TRequest : IStreamRequest<TResponse>
    {
        foreach (var processor in preProcessors)
        {
            await processor.ProcessAsync<TRequest, TResponse>(request, cancellationToken).ConfigureAwait(false);
        }

        await foreach (var item in next(cancellationToken).ConfigureAwait(false))
        {
            yield return item;
        }
    }
}
