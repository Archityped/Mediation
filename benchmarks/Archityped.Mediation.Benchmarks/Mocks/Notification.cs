namespace Archityped.Mediation.Benchmarks.Mocks;

public record Notification : INotification { }
public class NotificationHandler : INotificationHandler<Notification>
{
    public Task HandleAsync(Notification notification, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}