namespace Archityped.Mediation.Tests.Mocks;

public record MockStreamRequest(int Count, int Delay = 0) : IStreamRequest<int>;

public class MockStreamRequestHandler : IStreamRequestHandler<MockStreamRequest, int>
{
    public async IAsyncEnumerable<int> HandleAsync(MockStreamRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < request.Count; i++)
        {
            // Simulate async work
            await Task.Delay(request.Delay, cancellationToken);
            yield return i;

        }
    }
}
