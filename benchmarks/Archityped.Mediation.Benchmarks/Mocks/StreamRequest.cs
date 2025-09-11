using System.Runtime.CompilerServices;

namespace Archityped.Mediation.Benchmarks.Mocks;

public record StreamRequest(int Items) : IStreamRequest<int>;
public class StreamRequestHandler : IStreamRequestHandler<StreamRequest, int>
{
    public async IAsyncEnumerable<int> HandleAsync(StreamRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < request.Items; i++)
        {
            await Task.Yield();
            yield return i;
        }
    }
}