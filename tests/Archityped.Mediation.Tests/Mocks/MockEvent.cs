namespace Archityped.Mediation.Tests.Mocks;

public record MockEvent(string Message) : IEvent;

public class MockEventHandler : IEventHandler<MockEvent>
{
    public Task HandleAsync(MockEvent @event, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
