using System.ComponentModel;

namespace Archityped.Mediation;

/// <summary>
/// Represents a marker interface for notification handler types recognized by the mediator infrastructure.
/// </summary>
/// <remarks>
/// This interface serves as a base type for notification handler components and is intended for internal framework use.
/// Implement <see cref="INotificationHandler{TNotification}"/> for concrete notification handling logic instead of implementing this interface directly.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IBaseNotificationHandler
{
}