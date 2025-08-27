namespace Archityped.Mediation.Configuration;

/// <summary>
/// Represents a service descriptor for mediator components that extends <see cref="ServiceDescriptor"/> with mediator-specific metadata.
/// </summary>
internal class MediatorServiceDescriptor : ServiceDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorServiceDescriptor"/> class with an implementation type.
    /// </summary>
    /// <param name="componentType">The <see cref="MediatorComponentType"/> that categorizes the mediator component.</param>
    /// <param name="serviceType">The type of the service.</param>
    /// <param name="implementationType">The type of the implementation.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the service.</param>
    /// <returns>A new instance of the <see cref="MediatorServiceDescriptor"/> class configured with the specified parameters.</returns>
    public MediatorServiceDescriptor(MediatorComponentType componentType, Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Transient)
        : base(serviceType, implementationType, lifetime)
    {
        ComponentType = componentType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediatorServiceDescriptor"/> class with a factory function.
    /// </summary>
    /// <param name="componentType">The <see cref="MediatorComponentType"/> that categorizes the mediator component.</param>
    /// <param name="serviceType">The type of the service.</param>
    /// <param name="factory">A factory function for creating the service instance.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the service.</param>
    /// <returns>A new instance of the <see cref="MediatorServiceDescriptor"/> class configured with the specified parameters.</returns>
    public MediatorServiceDescriptor(MediatorComponentType componentType, Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
        : base(serviceType, factory, lifetime)
    {
        ComponentType = componentType;
    }

    /// <summary>
    /// Gets the mediator component type that categorizes the kind of mediator service.
    /// </summary>
    /// <returns>A <see cref="MediatorComponentType"/> value indicating the category of the mediator component.</returns>
    public MediatorComponentType ComponentType { get; private set; }
}