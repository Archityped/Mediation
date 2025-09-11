namespace Archityped.Mediation.Benchmarks.Mocks;

public record Request : IRequest<string> { }
public class RequestHandler : IRequestHandler<Request, string>
{
    public Task<string> HandleAsync(Request request, CancellationToken cancellationToken = default) =>
        Task.FromResult("ok");
}