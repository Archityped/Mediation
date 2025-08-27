namespace Archityped.Mediation;

/// <summary>
/// Represents a component that executes logic before a streaming request is handled in the mediation pipeline.
/// </summary>
public interface IStreamRequestPreProcessor: IBaseStreamRequestProcessor
{
    /// <summary>
    /// Asynchronously performs pre-processing logic for the specified streaming request before it is handled.
    /// </summary>
    /// <typeparam name="TRequest">The type of the streaming request being processed.</typeparam>
    /// <typeparam name="TResponse">The type of the streamed response elements produced by the handler.</typeparam>
    /// <param name="request">The streaming request instance to pre-process.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous pre-processing operation.</returns>
    ValueTask ProcessAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IStreamRequest<TResponse>;
}
