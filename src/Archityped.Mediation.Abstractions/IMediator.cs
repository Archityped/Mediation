namespace Archityped.Mediation;

/// <summary>
/// Represents a mediator that can send requests and publish notifications.
/// </summary>
public interface IMediator : IRequestSender, INotificationPublisher
{
}
