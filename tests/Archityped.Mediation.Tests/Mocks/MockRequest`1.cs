namespace Archityped.Mediation.Tests.Mocks;

public record MockRequestWithResponse(string Message) : IRequest<string>;

public class MockRequestHandlerWithResponse : IRequestHandler<MockRequestWithResponse, string>
{
    public Task<string> HandleAsync(MockRequestWithResponse request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(request.Message);
    }
}
