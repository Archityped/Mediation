using System.ComponentModel;

namespace Archityped.Mediation;

/// <summary>
/// Represents a marker interface for stream request message types recognized by the mediator infrastructure.
/// </summary>
/// <remarks>
/// This interface serves as a base type for stream request components and is intended for internal framework use.
/// Implement <see cref="IStreamRequest{TResponse}"/> for concrete stream request logic instead of implementing this interface directly.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IBaseStreamRequest : IMessage
{
}
