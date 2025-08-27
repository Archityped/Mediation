namespace Archityped.Mediation.Configuration;

public partial class MediatorConfiguration
{
    private static Type StreamRequestProcessorType => typeof(IStreamRequestPreProcessor);

    /// <summary>
    /// Adds a stream request processor registration using the specified implementation type.
    /// </summary>
    /// <param name="implementationType">The <see cref="Type"/> that implements <see cref="IStreamRequestPreProcessor"/> to register.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> to apply to the registration.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    public MediatorConfiguration AddStreamRequestProcessor(
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient)
        => AddService(implementationType, MediatorComponentType.StreamRequestProcessor, StreamRequestProcessorType, lifetime);

    /// <inheritdoc cref="AddStreamRequestProcessor(Type, ServiceLifetime)"/>
    /// <typeparam name="TProcessor">The concrete type that implements <see cref="IBaseStreamRequestProcessor"/>.</typeparam>
    public MediatorConfiguration AddStreamRequestProcessor<
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
    TProcessor>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TProcessor : IBaseStreamRequestProcessor
        => AddService(typeof(TProcessor), MediatorComponentType.StreamRequestProcessor, StreamRequestProcessorType, serviceLifetime);

    /// <summary>
    /// Adds a stream request processor registration using a factory delegate.
    /// </summary>
    /// <typeparam name="TProcessor">The concrete type that implements <see cref="IBaseStreamRequestProcessor"/>.</typeparam>
    /// <param name="factory">A delegate that creates an instance of <typeparamref name="TProcessor"/> using the provided <see cref="IServiceProvider"/>.</param>
    /// <param name="serviceLifetime">The <see cref="ServiceLifetime"/> to apply to the registration.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    public MediatorConfiguration AddStreamRequestProcessor<
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
    TProcessor>(Func<IServiceProvider, TProcessor> factory, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TProcessor : IBaseStreamRequestProcessor
        => AddService(factory, MediatorComponentType.StreamRequestProcessor, StreamRequestProcessorType, serviceLifetime);

}
