namespace Archityped.Mediation.Configuration;

/// <summary>
/// Represents the type of mediator component for service registration categorization.
/// </summary>
public enum MediatorComponentType
{
    /// <summary>
    /// Indicates a service for a notification handler component.
    /// </summary>
    NotificationHandler,

    /// <summary>
    /// Indicates a service for a request handler component.
    /// </summary>
    RequestHandler,

    /// <summary>
    /// Indicates a service for a request middleware component.
    /// </summary>
    RequestMiddleware,

    /// <summary>
    /// Indicates a service for a request processor component.
    /// </summary>
    RequestProcessor,

    /// <summary>
    /// Indicates a service for a stream request handler component.
    /// </summary>
    StreamRequestHandler,

    /// <summary>
    /// Indicates a service for a stream request middleware component.
    /// </summary>
    StreamRequestMiddleware,

    /// <summary>
    /// Indicates a service for a stream request pre-processor component executed before streaming starts.
    /// </summary>
    StreamRequestProcessor,
}
