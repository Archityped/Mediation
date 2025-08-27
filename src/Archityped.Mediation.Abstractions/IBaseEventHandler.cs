namespace Archityped.Mediation;

/// <summary>
/// Represents a marker interface for event handler types recognized by the mediator infrastructure.
/// </summary>
/// <remarks>
/// This interface serves as a base type for event handler components and is intended for internal framework use.
/// Implement <see cref="IEventHandler{TEvent}"/> for concrete event handling logic instead of implementing this interface directly.
/// </remarks>
public interface IBaseEventHandler
{
}