namespace Archityped.Mediation.Configuration;

/// <summary>
/// Specifies the kind of mediator component to register in the mediator configuration.
/// </summary>
public enum MediatorRegistrationKind
{
    /// <summary>
    /// Indicates a registration for an event handler component.
    /// </summary>
    EventHandler,

    /// <summary>
    /// Indicates a registration for a request handler component.
    /// </summary>
    RequestHandler,

    /// <summary>
    /// Indicates a registration for a request middleware component.
    /// </summary>
    RequestMiddleware,

    /// <summary>
    /// Indicates a registration for a stream request handler component.
    /// </summary>
    StreamRequestHandler,

    /// <summary>
    /// Indicates a registration for a stream request middleware component.
    /// </summary>
    StreamRequestMiddleware
}
