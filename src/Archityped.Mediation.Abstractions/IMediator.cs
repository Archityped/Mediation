namespace Archityped.Mediation;

/// <summary>
/// Represents a mediator that can send requests and publish events.
/// </summary>
public interface IMediator : IRequestSender, IEventPublisher
{ 
}
