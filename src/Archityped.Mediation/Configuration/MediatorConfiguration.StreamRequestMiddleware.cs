namespace Archityped.Mediation.Configuration;

public partial class MediatorConfiguration
{
    /// <summary>
    /// Adds a stream request middleware of the specified type to the configuration.
    /// </summary>
    /// <typeparam name="TMiddleware">
    /// The concrete middleware type to register. Must implement <see cref="IStreamRequestMiddleware"/>.
    /// </typeparam>
    /// <returns>
    /// The current <see cref="MediatorConfiguration"/> instance for method chaining.
    /// </returns>
    public MediatorConfiguration AddStreamRequestMiddleware<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        TMiddleware>()
    where TMiddleware : IStreamRequestMiddleware
        => Add(MediatorRegistrationKind.StreamRequestMiddleware, typeof(IStreamRequestMiddleware), typeof(TMiddleware));

    /// <summary>
    /// Adds a stream request middleware of the specified implementation type to the configuration.
    /// </summary>
    /// <param name="implementationType">
    /// The concrete type to register as stream request middleware. Must implement <see cref="IStreamRequestMiddleware"/>.
    /// </param>
    /// <returns>
    /// The current <see cref="MediatorConfiguration"/> instance for method chaining.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="implementationType"/> does not implement <see cref="IStreamRequestMiddleware"/>.
    /// </exception>
    public MediatorConfiguration AddStreamRequestMiddleware(
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
    Type implementationType) 
        => typeof(IStreamRequestMiddleware).IsAssignableFrom(implementationType)
            ? Add(MediatorRegistrationKind.RequestMiddleware, typeof(IStreamRequestMiddleware), implementationType)
            : throw new InvalidOperationException($"The type {implementationType.FullName} does not implement a valid stream request middleware interface.");
}
