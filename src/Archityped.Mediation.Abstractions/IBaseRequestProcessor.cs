using System.ComponentModel;

namespace Archityped.Mediation;

/// <summary>
/// Represents a marker interface for request processor types recognized by the mediator infrastructure.
/// </summary>
/// <remarks>
/// This interface is intended for internal framework use to enable discovery and registration of processor components
/// participating in the request execution pipeline. Implement <see cref="IRequestPreProcessor"/> (or other concrete
/// processor interfaces) for executable processing logic instead of implementing this interface directly.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IBaseRequestProcessor
{
}
