namespace Archityped.Mediation.Tests.Mocks;

public record MockRequest(string Message) : IRequest;

public class MockRequestHandler : IRequestHandler<MockRequest>
{
    public Task HandleAsync(MockRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}