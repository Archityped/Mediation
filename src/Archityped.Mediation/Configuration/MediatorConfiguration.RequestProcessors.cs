namespace Archityped.Mediation.Configuration;

public partial class MediatorConfiguration
{
    private static Type[] RequestProcessorTypes =>
    [
        typeof(IRequestPreProcessor),
        typeof(IRequestPostProcessor),
    ];

    /// <summary>
    /// Adds a request processor service using the specified implementation type.
    /// </summary>
    /// <param name="implementationType">The concrete type that implements <see cref="IBaseRequestProcessor"/>.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> to apply to the registration. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    public MediatorConfiguration AddRequestProcessor(
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
            Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient)
           => AddService(implementationType, MediatorComponentType.RequestProcessor, RequestProcessorTypes, lifetime);

    /// <inheritdoc cref="AddRequestProcessor(Type, ServiceLifetime)"/>
    /// <typeparam name="TProcessor">The concrete type that implements <see cref="IBaseRequestProcessor"/>.</typeparam>
    public MediatorConfiguration AddRequestProcessor<
#if NET6_0_OR_GREATER
        [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
    TProcessor>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TProcessor : IBaseRequestProcessor
        => AddService(typeof(TProcessor), MediatorComponentType.RequestProcessor, RequestProcessorTypes, lifetime);

    /// <summary>
    /// Adds a request processor service using a factory delegate.
    /// </summary>
    /// <typeparam name="TProcessor">The concrete type that implements <see cref="IBaseRequestProcessor"/>.</typeparam>
    /// <param name="factory">A delegate that creates an instance of <typeparamref name="TProcessor"/> using the provided <see cref="IServiceProvider"/>.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> to apply to the registration. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    public MediatorConfiguration AddRequestProcessor<
#if NET6_0_OR_GREATER
        [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
    TProcessor>(Func<IServiceProvider, TProcessor> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TProcessor : IBaseRequestProcessor
        => AddService(factory, MediatorComponentType.RequestProcessor, RequestProcessorTypes, lifetime);
}
