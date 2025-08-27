using System.ComponentModel;

namespace Archityped.Mediation;

/// <summary>
/// Represents a marker interface for stream request processor types recognized by the mediator infrastructure.
/// </summary>
/// <remarks>
/// This interface is intended for internal framework use to enable discovery and registration of components that perform
/// processing logic associated with streaming request execution. Implement <see cref="IStreamRequestPreProcessor"/> (or other
/// concrete stream processor interfaces) for executable processing logic instead of implementing this interface directly.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IBaseStreamRequestProcessor
{
}