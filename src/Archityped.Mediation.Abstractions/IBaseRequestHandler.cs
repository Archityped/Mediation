using System.ComponentModel;

namespace Archityped.Mediation;

/// <summary>
/// Represents the marker interface for request handlers in the mediation pipeline.
/// </summary>
/// <remarks>
/// This interface serves as a base type for request handler components and is intended for internal framework use.
/// Implement <see cref="IRequestHandler{TRequest}"/> or <see cref="IRequestHandler{TRequest, TResponse}"/> instead of this interface directly.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IBaseRequestHandler
{
}
