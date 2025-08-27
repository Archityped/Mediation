using System.Runtime.CompilerServices;

namespace Archityped.Mediation.Configuration;

public partial class MediatorConfiguration
{
    /// <summary>
    /// Adds a request middleware of the specified type to the configuration using a factory delegate.
    /// </summary>
    /// <typeparam name="TMiddleware">
    /// The concrete middleware type to register. Must implement <see cref="IRequestMiddleware"/>.
    /// </typeparam>
    /// <param name="factory">
    /// A factory delegate used to create instances of <typeparamref name="TMiddleware"/>. If <see langword="null"/>, the default constructor is used.
    /// </param>
    /// <returns>
    /// The current <see cref="MediatorConfiguration"/> instance for method chaining.
    /// </returns>
    public MediatorConfiguration AddRequestMiddleware<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        TMiddleware>(Func<IServiceProvider, TMiddleware>? factory)
        where TMiddleware : IRequestMiddleware
            => Add(MediatorRegistrationKind.RequestMiddleware, typeof(IRequestMiddleware), typeof(TMiddleware), Unsafe.As<Func<IServiceProvider, object>>(factory));

    /// <summary>
    /// Adds a request middleware of the specified type to the configuration.
    /// </summary>
    /// <typeparam name="TMiddleware">
    /// The concrete middleware type to register. Must implement <see cref="IRequestMiddleware"/>.
    /// </typeparam>
    /// <returns>
    /// The current <see cref="MediatorConfiguration"/> instance for method chaining.
    /// </returns>
    public MediatorConfiguration AddRequestMiddleware<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        TMiddleware>()
        where TMiddleware : IRequestMiddleware
            => Add(MediatorRegistrationKind.RequestHandler, typeof(IRequestMiddleware), typeof(TMiddleware));

    /// <summary>
    /// Adds a request middleware of the specified implementation type to the configuration.
    /// </summary>
    /// <param name="implementationType">
    /// The concrete type to register as request middleware. Must implement <see cref="IRequestMiddleware"/>.
    /// </param>
    /// <returns>
    /// The current <see cref="MediatorConfiguration"/> instance for method chaining.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="implementationType"/> does not implement <see cref="IRequestMiddleware"/>.
    /// </exception>
    public MediatorConfiguration AddRequestMiddleware(
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        Type implementationType) 
        => typeof(IRequestMiddleware).IsAssignableFrom(implementationType)
            ? Add(MediatorRegistrationKind.RequestMiddleware, typeof(IRequestMiddleware), implementationType)
            : throw new InvalidOperationException($"The type {implementationType.FullName} does not implement a valid request middleware interface.");

}
