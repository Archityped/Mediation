namespace Archityped.Mediation.Configuration;

public partial class MediatorConfiguration
{
    private static Type RequestStreamHandlerType => typeof(IStreamRequestHandler<,>);

    /// <summary>
    /// Adds a stream request handler service using the specified implementation type.
    /// </summary>
    /// <param name="implementationType">The concrete type that implements <see cref="IBaseStreamRequestHandler"/>.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> to apply to the registration. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    public MediatorConfiguration AddStreamRequestHandler(
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient)
        => AddService(implementationType, MediatorComponentType.StreamRequestHandler, RequestStreamHandlerType, lifetime);

    /// <inheritdoc cref="AddStreamRequestHandler(Type, ServiceLifetime)"/>
    /// <typeparam name="TStreamRequestHandler">The concrete type that implements <see cref="IBaseStreamRequestHandler"/>.</typeparam>
    public MediatorConfiguration AddStreamRequestHandler<
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
    TStreamRequestHandler>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TStreamRequestHandler : IBaseStreamRequestHandler
        => AddService(typeof(TStreamRequestHandler), MediatorComponentType.StreamRequestHandler, RequestStreamHandlerType, lifetime);

    /// <summary>
    /// Adds a stream request handler service using a factory delegate.
    /// </summary>
    /// <typeparam name="TStreamRequestHandler">The concrete type that implements <see cref="IBaseStreamRequestHandler"/>.</typeparam>
    /// <param name="factory">A delegate that creates an instance of <typeparamref name="TStreamRequestHandler"/> using the provided <see cref="IServiceProvider"/>.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> to apply to the registration. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    public MediatorConfiguration AddStreamRequestHandler<
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
    TStreamRequestHandler>(Func<IServiceProvider, TStreamRequestHandler> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TStreamRequestHandler : class
        => AddService(factory, MediatorComponentType.StreamRequestHandler, RequestStreamHandlerType, lifetime);
}
