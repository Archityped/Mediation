namespace Archityped.Mediation.Benchmarks.Mocks;

public record VoidRequest : IRequest { }
public class VoidRequestHandler : IRequestHandler<VoidRequest>
{
    public Task HandleAsync(VoidRequest request, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}