namespace Archityped.Mediation.Configuration;

public partial class MediatorConfiguration
{
    private static Type NotificationHandlerType => typeof(INotificationHandler<>);

    /// <summary>
    /// Adds a notification handler service using the specified implementation type.
    /// </summary>
    /// <param name="implementationType">The concrete type that implements <see cref="IBaseNotificationHandler"/>.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> to apply to the registration. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    public MediatorConfiguration AddNotificationHandler(
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient)
        => AddService(implementationType, MediatorComponentType.NotificationHandler, NotificationHandlerType, lifetime);

    /// <inheritdoc cref="AddNotificationHandler(Type, ServiceLifetime)"/>
    /// <typeparam name="TNotificationHandler">The concrete type that implements <see cref="IBaseNotificationHandler"/>.</typeparam>
    public MediatorConfiguration AddNotificationHandler<
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
    TNotificationHandler>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TNotificationHandler : IBaseNotificationHandler
        => AddService(typeof(TNotificationHandler), MediatorComponentType.NotificationHandler, NotificationHandlerType, lifetime);

    /// <summary>
    /// Adds a notification handler service using a factory delegate.
    /// </summary>
    /// <typeparam name="TNotificationHandler">The concrete type that implements <see cref="IBaseNotificationHandler"/>.</typeparam>
    /// <param name="factory">A delegate that creates an instance of <typeparamref name="TNotificationHandler"/> using the provided <see cref="IServiceProvider"/>.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> to apply to the registration. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    public MediatorConfiguration AddNotificationHandler<
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
    TNotificationHandler>(Func<IServiceProvider, TNotificationHandler> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TNotificationHandler : IBaseNotificationHandler
        => AddService(factory, MediatorComponentType.NotificationHandler, NotificationHandlerType, lifetime);
}
