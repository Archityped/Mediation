namespace Archityped.Mediation;

/// <summary>
/// Represents the base marker interface for all messages in the mediation pipeline.
/// </summary>
/// <remarks>
/// This interface provides type safety and enables polymorphic handling
/// of different message categories. This interface is intended for internal framework use and should not be
/// implemented directly. Instead, implement more specific interfaces such as  <see cref="IEvent"/>, 
/// <see cref="IRequest"/>, <see cref="IRequest{TResponse}"/>, or <see cref="IStreamRequest{TResponse}"/>.
/// </remarks>
public interface IMessage
{
}