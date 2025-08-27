namespace Archityped.Mediation.Configuration;

/// <summary>
/// Represents a service registration entry for mediator components that contains the service type, implementation type, and optional factory.
/// </summary>
public readonly record struct MediatorRegistrationEntry
{
    /// <summary>
    /// Gets the kind of mediator component being registered.
    /// </summary>
    /// <returns>A <see cref="MediatorRegistrationKind"/> value indicating the kind of the mediator component.</returns>
    public required MediatorRegistrationKind Kind { get; init; }

    /// <summary>
    /// Gets the service type that will be registered with the dependency injection container.
    /// </summary>
    /// <returns>The <see cref="Kind"/> that represents the service interface or abstract type.</returns>
    public required Type ServiceType { get; init; }

    /// <summary>
    /// Gets the concrete implementation type that will be instantiated by the dependency injection container.
    /// </summary>
    /// <returns>The <see cref="Kind"/> that represents the concrete implementation class.</returns>
    public required Type ImplementationType { get; init; }

    /// <summary>
    /// Gets the optional factory function used to create instances of the implementation type.
    /// </summary>
    /// <returns>
    /// A <see cref="Func{T, TResult}"/> that takes an <see cref="IServiceProvider"/> and returns an instance of the implementation,
    /// or <see langword="null"/> if the default constructor-based instantiation should be used.
    /// </returns>
    public Func<IServiceProvider, object>? Factory { get; init; }
}
