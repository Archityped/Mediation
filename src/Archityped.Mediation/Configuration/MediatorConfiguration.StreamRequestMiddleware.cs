namespace Archityped.Mediation.Configuration;

public partial class MediatorConfiguration
{
    private static Type StreamRequestMiddlewareType => typeof(IStreamRequestMiddleware);

    /// <summary>
    /// Adds a stream request middleware registration using the specified implementation type.
    /// </summary>
    /// <param name="implementationType">The concrete type that implements <see cref="IStreamRequestMiddleware"/>.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> to apply to the registration. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    public MediatorConfiguration AddStreamRequestMiddleware(
#if NET6_0_OR_GREATER
   [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient)
        => AddService(implementationType, MediatorComponentType.StreamRequestMiddleware, StreamRequestMiddlewareType, lifetime);

    /// <inheritdoc cref="AddStreamRequestMiddleware(Type, ServiceLifetime)"/>
    /// <typeparam name="TMiddleware">The concrete type that implements <see cref="IStreamRequestMiddleware"/>.</typeparam>
    public MediatorConfiguration AddStreamRequestMiddleware<
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
    TMiddleware>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TMiddleware : IStreamRequestMiddleware
        => AddService(typeof(TMiddleware), MediatorComponentType.StreamRequestMiddleware, StreamRequestMiddlewareType, lifetime);

    /// <summary>
    /// Adds a stream request middleware registration using a factory delegate.
    /// </summary>
    /// <typeparam name="TMiddleware">The concrete type that implements <see cref="IStreamRequestMiddleware"/>.</typeparam>
    /// <param name="factory">A delegate that creates an instance of <typeparamref name="TMiddleware"/> using the provided <see cref="IServiceProvider"/>.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> to apply to the registration.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    public MediatorConfiguration AddStreamRequestMiddleware<
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
    TMiddleware>(Func<IServiceProvider, TMiddleware> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TMiddleware : IStreamRequestMiddleware
        => AddService(factory, MediatorComponentType.StreamRequestMiddleware, StreamRequestMiddlewareType, lifetime);
}
