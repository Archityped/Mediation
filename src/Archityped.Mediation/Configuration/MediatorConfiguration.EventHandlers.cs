namespace Archityped.Mediation.Configuration;

public partial class MediatorConfiguration
{
    /// <summary>
    /// Adds an event handler of the specified type to the configuration.
    /// </summary>
    /// <typeparam name="TEventHandler">
    /// The concrete event handler type to register. Must implement <see cref="IEventHandler{TEvent}"/>.
    /// </typeparam>
    /// <returns>
    /// The current <see cref="MediatorConfiguration"/> instance for method chaining.
    /// </returns>
    public MediatorConfiguration AddEventHandler<
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        TEventHandler>()
        where TEventHandler : IBaseEventHandler
    {
        return AddEventHandler(typeof(TEventHandler));
    }

    /// <summary>
    /// Adds an event handler of the specified implementation type to the configuration.
    /// </summary>
    /// <param name="implementationType">
    /// The concrete type to register as an event handler. Must implement <see cref="IEventHandler{TEvent}"/> for some event type.
    /// </param>
    /// <returns>
    /// The current <see cref="MediatorConfiguration"/> instance for method chaining.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="implementationType"/> does not implement a valid event handler interface.
    /// </exception>
    public MediatorConfiguration AddEventHandler(
#if NET8_0_OR_GREATER
    [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        Type implementationType)
    {
        var serviceType = implementationType.GetInterface(typeof(IEventHandler<>).Name);
        return serviceType is not null
            ? Add(MediatorRegistrationKind.EventHandler, serviceType, implementationType)
            : throw new InvalidOperationException($"The type {implementationType.FullName} does not implement a valid event handler interface.");
    }
}
