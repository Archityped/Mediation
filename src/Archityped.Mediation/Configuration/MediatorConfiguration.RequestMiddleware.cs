namespace Archityped.Mediation.Configuration;

public partial class MediatorConfiguration
{
    private static Type RequestMiddlewareType => typeof(IRequestMiddleware);

    /// <summary>
    /// Adds a request middleware service using the specified implementation type.
    /// </summary>
    /// <param name="implementationType">The concrete type that implements <see cref="IRequestMiddleware"/>.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> to apply to the registration. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    public MediatorConfiguration AddRequestMiddleware(
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient)
        => AddService(implementationType, MediatorComponentType.RequestMiddleware, RequestMiddlewareType, lifetime);

    /// <inheritdoc cref="AddRequestMiddleware(Type, ServiceLifetime)"/>
    /// <typeparam name="TMiddleware">The concrete type that implements <see cref="IRequestMiddleware"/>.</typeparam>
    public MediatorConfiguration AddRequestMiddleware<
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
    TMiddleware>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TMiddleware : IRequestMiddleware
        => AddService(typeof(TMiddleware), MediatorComponentType.RequestMiddleware, RequestMiddlewareType, lifetime);

    /// <summary>
    /// Adds a request middleware service using a factory delegate.
    /// </summary>
    /// <typeparam name="TMiddleware">The concrete type that implements <see cref="IRequestMiddleware"/>.</typeparam>
    /// <param name="factory">A delegate that creates an instance of <typeparamref name="TMiddleware"/> using the provided <see cref="IServiceProvider"/>.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> to apply to the registration. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    public MediatorConfiguration AddRequestMiddleware<
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
    TMiddleware>(Func<IServiceProvider, TMiddleware> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TMiddleware : IRequestMiddleware
        => AddService(factory, MediatorComponentType.RequestMiddleware, RequestMiddlewareType, lifetime);
}
