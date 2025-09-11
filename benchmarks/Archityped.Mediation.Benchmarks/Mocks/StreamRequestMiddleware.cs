using System.Runtime.CompilerServices;

namespace Archityped.Mediation.Benchmarks.Mocks;

public class StreamRequestMiddleware : IStreamRequestMiddleware
{
    public async IAsyncEnumerable<TResponse> InvokeAsync<TRequest, TResponse>(TRequest request, StreamRequestHandlerDelegate<TResponse> next, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TRequest : IStreamRequest<TResponse>
    {
        await foreach (var item in next(cancellationToken).WithCancellation(cancellationToken))
            yield return item;
    }
}