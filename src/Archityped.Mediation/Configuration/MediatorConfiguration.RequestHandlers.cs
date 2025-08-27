namespace Archityped.Mediation.Configuration;

public partial class MediatorConfiguration
{
    /// <summary>
    /// Adds a request handler of the specified type to the configuration.
    /// </summary>
    /// <typeparam name="TRequestHandler">
    /// The concrete request handler type to register. Must implement <see cref="IBaseRequestHandler"/>.
    /// </typeparam>
    /// <returns>
    /// The current <see cref="MediatorConfiguration"/> instance for method chaining.
    /// </returns>
    public MediatorConfiguration AddRequestHandler<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        TRequestHandler>()
        where TRequestHandler : IBaseRequestHandler
    {
        return AddRequestHandler(typeof(TRequestHandler));
    }

    /// <summary>
    /// Adds a request handler of the specified implementation type to the configuration.
    /// </summary>
    /// <param name="implementationType">
    /// The concrete type to register as a request handler. Must implement <see cref="IRequestHandler{TRequest}"/> or <see cref="IRequestHandler{TRequest, TResponse}"/> for some request type.
    /// </param>
    /// <returns>
    /// The current <see cref="MediatorConfiguration"/> instance for method chaining.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="implementationType"/> does not implement a valid request handler interface.
    /// </exception>
    public MediatorConfiguration AddRequestHandler(
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        Type implementationType)
    {
        var serviceType = implementationType.GetInterface(typeof(IRequestHandler<>).Name) ?? implementationType.GetInterface(typeof(IRequestHandler<,>).Name);
        return serviceType is not null
            ? Add(MediatorRegistrationKind.RequestHandler, serviceType, implementationType)
            : throw new InvalidOperationException($"The type {implementationType.FullName} does not implement a valid request handler interface.");
    }
}