namespace Archityped.Mediation;

/// <summary>
/// Represents a marker interface for stream request handler types recognized by the mediator infrastructure.
/// </summary>
/// <remarks>
/// This interface serves as a base type for stream request handler components and is intended for internal framework use.
/// Implement <see cref="IStreamRequestHandler{TRequest, TResponse}"/> for concrete stream request handling logic instead of implementing this interface directly.
/// </remarks>
public interface IBaseStreamRequestHandler
{
}