using System.ComponentModel;

namespace Archityped.Mediation;

/// <summary>
/// Represents the marker interface for requests in the mediation pipeline.
/// </summary>
/// <remarks>
/// This interface serves as a base type for request components and is intended for internal framework use.
/// Implement <see cref="IRequest"/> or <see cref="IRequest{TResponse}"/> instead of this interface directly.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IBaseRequest : IMessage
{
}
