namespace Archityped.Mediation.Configuration;

/// <summary>
/// Represents a configuration builder for registering mediator components, including handlers, middleware, and processors, with the dependency injection container.
/// </summary>
public partial class MediatorConfiguration
{
    private readonly List<MediatorServiceDescriptor> _serviceDescriptors = [];

    /// <summary>
    /// Gets or sets the <see cref="ServiceLifetime"/> used for mediator instance when a specific lifetime is not otherwise supplied.
    /// </summary>
    /// <returns>A <see cref="ServiceLifetime"/> value applied as the default lifetime for registered mediator components.</returns>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

    /// <summary>
    /// Gets the implementation type used for the <see cref="IMediator"/> service.
    /// </summary>
    /// <returns>A <see cref="Type"/> representing the concrete mediator implementation registered for <see cref="IMediator"/>.</returns>
    public Type ImplementationType { get; } = typeof(Mediator);

    /// <summary>
    /// Gets the collection of mediator component registrations configured for this instance.
    /// </summary>
    /// <returns>An <see cref="IReadOnlyCollection{T}"/> of mediator component registrations registered for this instance.</returns>
    internal IList<MediatorServiceDescriptor> ServiceRegistrations => _serviceDescriptors;

    /// <summary>
    /// Adds a service registration for a mediator component with the specified interface types.
    /// </summary>
    /// <param name="implementationType">The concrete type that implements one of the target interfaces.</param>
    /// <param name="kind">The <see cref="MediatorComponentType"/> that categorizes the mediator component.</param>
    /// <param name="serviceTypes">An array of target interface types to match against.</param>
    /// <param name="serviceLifetime">The <see cref="ServiceLifetime"/> to apply to the registration.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="implementationType"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The implementation type does not implement any of the expected interfaces.</exception>
    private MediatorConfiguration AddService(
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        Type implementationType, MediatorComponentType kind, Type[] serviceTypes, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (implementationType is null)
        {
            throw new ArgumentNullException(nameof(implementationType));
        }

        var descriptor = implementationType.TryFindClosedInterface(serviceTypes, out var serviceType)
            ? new MediatorServiceDescriptor(kind, serviceType!, implementationType, serviceLifetime)
            : throw new InvalidOperationException($"The type {implementationType.FullName} does not implement a valid interface of the expected types: {string.Join(", ", serviceTypes.Select(t => t.FullName))}");

        _serviceDescriptors.Add(descriptor);
        return this;
    }

    /// <summary>
    /// Adds a service registration for a mediator component with the specified interface type.
    /// </summary>
    /// <param name="implementationType">The concrete type that implements the target interface.</param>
    /// <param name="kind">The <see cref="MediatorComponentType"/> that categorizes the mediator component.</param>
    /// <param name="serviceType">The target interface type to match against.</param>
    /// <param name="serviceLifetime">The <see cref="ServiceLifetime"/> to apply to the registration.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="implementationType"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The implementation type does not implement the expected interface.</exception>
    private MediatorConfiguration AddService(
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        Type implementationType, MediatorComponentType kind, Type serviceType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (implementationType is null)
        {
            throw new ArgumentNullException(nameof(implementationType));
        }

        var descriptor = implementationType.TryFindClosedInterface(serviceType, out var assignedServiceType)
            ? new MediatorServiceDescriptor(kind, assignedServiceType!, implementationType, serviceLifetime)
            : throw new InvalidOperationException($"The type {implementationType.FullName} does not implement a valid interface of the expected type: {serviceType.FullName}");

        _serviceDescriptors.Add(descriptor);
        return this;
    }

    /// <summary>
    /// Adds a service registration for a mediator component using a factory delegate with the specified interface types.
    /// </summary>
    /// <typeparam name="T">The concrete type that implements one of the target interfaces.</typeparam>
    /// <param name="factory">A delegate that creates an instance of <typeparamref name="T"/> using the provided <see cref="IServiceProvider"/>.</param>
    /// <param name="kind">The <see cref="MediatorComponentType"/> that categorizes the mediator component.</param>
    /// <param name="targetTypes">An array of target interface types to match against.</param>
    /// <param name="serviceLifetime">The <see cref="ServiceLifetime"/> to apply to the registration.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The implementation type does not implement any of the expected interfaces.</exception>
    private MediatorConfiguration AddService<
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
    T>(Func<IServiceProvider, T> factory, MediatorComponentType kind, Type[] targetTypes, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        var implementationType = typeof(T);
        var descriptor = implementationType.TryFindClosedInterface(targetTypes, out var serviceType)
            ? new MediatorServiceDescriptor(kind, serviceType!, Unsafe.As<Func<IServiceProvider, object>>(factory), serviceLifetime)
            : throw new InvalidOperationException($"The type {implementationType.FullName} does not implement a valid interface of the expected types: {string.Join(", ", targetTypes.Select(t => t.FullName))}");

        _serviceDescriptors.Add(descriptor);
        return this;
    }

    /// <summary>
    /// Adds a service registration for a mediator component using a factory delegate with the specified interface type.
    /// </summary>
    /// <typeparam name="T">The concrete type that implements the target interface.</typeparam>
    /// <param name="factory">A delegate that creates an instance of <typeparamref name="T"/> using the provided <see cref="IServiceProvider"/>.</param>
    /// <param name="kind">The <see cref="MediatorComponentType"/> that categorizes the mediator component.</param>
    /// <param name="targetType">The target interface type to match against.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> to apply to the registration.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="factory"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The implementation type does not implement the expected interface.</exception>
    private MediatorConfiguration AddService<
#if NET6_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
    T>(Func<IServiceProvider, T> factory, MediatorComponentType kind, Type targetType, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        var implementationType = typeof(T);
        var descriptor = implementationType.TryFindClosedInterface(targetType, out var serviceType)
            ? new MediatorServiceDescriptor(kind, serviceType!, Unsafe.As<Func<IServiceProvider, object>>(factory), lifetime)
            : throw new InvalidOperationException($"The type {implementationType.FullName} does not implement a valid interface of the expected type: {targetType.FullName}");

        _serviceDescriptors.Add(descriptor);
        return this;
    }
}
