namespace Archityped.Mediation.Configuration;

public partial class MediatorConfiguration
{
    private static Type[] RequestHandlerTypes =>
    [
        typeof(IRequestHandler<>),
        typeof(IRequestHandler<,>),
    ];

    /// <summary>
    /// Adds a request handler service using the specified implementation type.
    /// </summary>
    /// <param name="implementationType">The concrete type that implements <see cref="IBaseRequestHandler"/>.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> to apply to the registration. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    public MediatorConfiguration AddRequestHandler(
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient)
        => AddService(implementationType, MediatorComponentType.RequestHandler, RequestHandlerTypes, lifetime);

    /// <inheritdoc cref="AddRequestHandler(Type, ServiceLifetime)"/>
    /// <typeparam name="TRequestHandler">The concrete type that implements <see cref="IBaseRequestHandler"/>.</typeparam>
    public MediatorConfiguration AddRequestHandler<
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
    TRequestHandler>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TRequestHandler : IBaseRequestHandler
        => AddService(typeof(TRequestHandler), MediatorComponentType.RequestHandler, RequestHandlerTypes, lifetime);

    /// <summary>
    /// Adds a request handler service using a factory delegate.
    /// </summary>
    /// <typeparam name="TRequestHandler">The concrete type that implements <see cref="IBaseRequestHandler"/>.</typeparam>
    /// <param name="factory">A delegate that creates an instance of <typeparamref name="TRequestHandler"/> using the provided <see cref="IServiceProvider"/>.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> to apply to the registration. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    public MediatorConfiguration AddRequestHandler<
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
    TRequestHandler>(Func<IServiceProvider, TRequestHandler> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TRequestHandler : IBaseRequestHandler
        => AddService(factory, MediatorComponentType.RequestHandler, RequestHandlerTypes, lifetime);
}