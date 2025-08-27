using System.ComponentModel;

namespace Archityped.Mediation;

/// <summary>
/// Represents the marker interface for middleware components in the mediation pipeline.
/// </summary>
/// <remarks>
/// This interface serves as a base type for middleware components and is intended for internal framework use.
/// Implement <see cref="IRequestMiddleware{TRequest, TResponse}"/> instead of this interface directly.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IBaseRequestMiddleware
{
}
