namespace Archityped.Mediation.Configuration;

public partial class MediatorConfiguration
{
    /// <summary>
    /// Adds a stream request handler of the specified type to the configuration.
    /// </summary>
    /// <typeparam name="TStreamRequestHandler">
    /// The concrete stream request handler type to register. Must implement <see cref="IBaseStreamRequestHandler"/>.
    /// </typeparam>
    /// <returns>
    /// The current <see cref="MediatorConfiguration"/> instance for method chaining.
    /// </returns>
    public MediatorConfiguration AddStreamRequestHandler<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        TStreamRequestHandler>()
        where TStreamRequestHandler : IBaseStreamRequestHandler
        => AddStreamRequestHandler(typeof(TStreamRequestHandler));

    /// <summary>
    /// Adds a stream request handler of the specified implementation type to the configuration.
    /// </summary>
    /// <param name="implementationType">
    /// The concrete type to register as a stream request handler. Must implement <see cref="IStreamRequestHandler{TRequest, TResponse}"/> for some request and response types.
    /// </param>
    /// <returns>
    /// The current <see cref="MediatorConfiguration"/> instance for method chaining.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="implementationType"/> does not implement a valid stream request handler interface.
    /// </exception>
    public MediatorConfiguration AddStreamRequestHandler(
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        Type implementationType)
    {
        var serviceType = implementationType.GetInterface(typeof(IStreamRequestHandler<,>).Name);
        return serviceType is not null
            ? Add(MediatorRegistrationKind.StreamRequestHandler, serviceType, implementationType)
            : throw new InvalidOperationException($"The type {implementationType.FullName} does not implement a valid stream request handler interface.");
    }
}
