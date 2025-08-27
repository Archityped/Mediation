namespace Archityped.Mediation.Configuration;

public partial class MediatorConfiguration
{
    internal ServiceDescriptor? NotificationPublisherRegistration { get; set; }

    /// <summary>
    /// Sets the notification publisher using a factory delegate.
    /// </summary>
    /// <typeparam name="TNotificationPublisher">The concrete type that implements <see cref="INotificationPublisher"/>.</typeparam>
    /// <param name="factory">A delegate that creates an instance of <typeparamref name="TNotificationPublisher"/> using the provided <see cref="IServiceProvider"/>.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <see langword="null"/>.</exception>
    public MediatorConfiguration SetNotificationPublisher<TNotificationPublisher>(Func<IServiceProvider, TNotificationPublisher> factory)
        where TNotificationPublisher : INotificationPublisher
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        NotificationPublisherRegistration = new ServiceDescriptor(typeof(INotificationPublisher), factory, Lifetime);
        return this;
    }

    /// <summary>
    /// Sets the notification publisher using the specified implementation type.
    /// </summary>
    /// <typeparam name="TImplementation">The concrete type that implements <see cref="INotificationPublisher"/>.</typeparam>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    public MediatorConfiguration SetNotificationPublisher<TImplementation>()
        where TImplementation : INotificationPublisher
        => SetNotificationPublisher(typeof(TImplementation));

    /// <summary>
    /// Sets the notification publisher using the specified implementation type.
    /// </summary>
    /// <param name="implementationType">The concrete type that implements <see cref="INotificationPublisher"/>.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="implementationType"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The specified type does not implement <see cref="INotificationPublisher"/>.</exception>
    public MediatorConfiguration SetNotificationPublisher(Type implementationType)
    {
        if (implementationType is null)
        {
            throw new ArgumentNullException(nameof(implementationType));
        }

        NotificationPublisherRegistration = implementationType.TryFindClosedInterface(typeof(INotificationPublisher), out var serviceType)
           ? new ServiceDescriptor(serviceType!, implementationType, Lifetime)
            : throw new InvalidOperationException($"The type {implementationType.FullName} does not implement a valid interface of the expected type: {typeof(INotificationPublisher).FullName}");

        return this;
    }
}