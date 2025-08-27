using Archityped.Mediation.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Archityped.Mediation.Configuration;

/// <summary>
/// Represents a configuration builder for registering mediator components, including handlers and middleware, with the dependency injection container.
/// </summary>
public partial class MediatorConfiguration
{
    private readonly HashSet<MediatorRegistrationEntry> _entries = [];

    /// <summary>
    /// Gets the <see cref="ServiceLifetime"/> used for mediator component registrations.
    /// </summary>
    /// <returns>
    /// A <see cref="ServiceLifetime"/> value that indicates the lifetime with which mediator components are registered.
    /// </returns>
    public ServiceLifetime ServiceLifetime { get; } = ServiceLifetime.Scoped;

    /// <summary>
    /// Gets the implementation type of the mediator interface.
    /// </summary>
    /// <returns>
    /// A <see cref="Type"/> representing the concrete implementation of <see cref="IMediator"/>.
    /// </returns>
    public Type ImplementationType { get; } = typeof(Mediator);

    /// <summary>
    /// Gets the collection of mediator registration entries configured for this instance.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="MediatorRegistrationEntry"/> representing the registered mediator components.
    /// </returns>
    public IEnumerable<MediatorRegistrationEntry> Registrations => _entries;

    /// <summary>
    /// Adds a mediator component registration entry to the configuration.
    /// </summary>
    /// <param name="kind">The type of mediator registration to add.</param>
    /// <param name="serviceType">The service type to register.</param>
    /// <param name="implementationType">The concrete implementation type to register.</param>
    /// <param name="factory">
    /// An optional factory function for creating the implementation instance. If <see langword="null"/>, the default constructor is used.
    /// </param>
    /// <returns>
    /// The current <see cref="MediatorConfiguration"/> instance for method chaining.
    /// </returns>
    private MediatorConfiguration Add(MediatorRegistrationKind kind, Type serviceType, Type implementationType, Func<IServiceProvider, object>? factory = null)
    {
        _entries.Add(
           new MediatorRegistrationEntry
           {
               Kind = kind,
               ServiceType = serviceType,
               ImplementationType = implementationType,
               Factory = factory
           });

        return this;
    }

    /// <summary>
    /// Adds all eligible mediator component types from the specified assembly to the configuration.
    /// </summary>
    /// <param name="assembly">The assembly to scan for mediator component types.</param>
    /// <returns>
    /// <see langword="true"/> if at least one type was registered; otherwise, <see langword="false"/>.
    /// </returns>
    internal bool Add(Assembly assembly)
    {
        bool added = false;
        foreach (var type in assembly.ExportedTypes)
        {
            added |= Add(type);
        }

        return added;
    }

    /// <summary>
    /// Adds a mediator component type to the configuration if it implements a recognized mediator interface.
    /// </summary>
    /// <param name="type">The type to analyze and potentially register as a mediator component.</param>
    /// <returns>
    /// <see langword="true"/> if the type was successfully registered; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Skips abstract types and interfaces. Registers types that implement
    /// <see cref="IEventHandler{TEvent}"/>, <see cref="IRequestHandler{TRequest, TResponse}"/>, 
    /// <see cref="IRequestHandler{TRequest}"/>, <see cref="IStreamRequestHandler{TRequest, TResponse}"/>,
    /// <see cref="IStreamRequest{TResponse}"/>, <see cref="IRequestMiddleware"/>, or <see cref="IStreamRequestMiddleware"/>.
    /// </remarks>
    internal bool Add(Type type)
    {
        if (type.IsInterface | type.IsAbstract)
        {
            return false;
        }

        ReadOnlySpan<Type> interfaces = type.GetInterfaces();
        int assignments = interfaces.Length;

        foreach (var @interface in interfaces)
        {
            if (@interface.IsGenericType)
            {
                var genericType = @interface.GetGenericTypeDefinition();
                if (genericType == typeof(IEventHandler<>))
                {
                    Add(MediatorRegistrationKind.RequestHandler, @interface, type);
                }
                else if (genericType == typeof(IRequestHandler<,>))
                {
                    Add(MediatorRegistrationKind.RequestHandler, @interface, type);
                }
                else if (genericType == typeof(IRequestHandler<>))
                {
                    Add(MediatorRegistrationKind.RequestHandler, @interface, type);
                }
                else if (genericType == typeof(IStreamRequestHandler<,>))
                {
                    Add(MediatorRegistrationKind.StreamRequestHandler, @interface, type);
                }
                else if (genericType == typeof(IStreamRequest<>))
                {
                    Add(MediatorRegistrationKind.StreamRequestHandler, @interface, type);
                }
                else
                {
                    assignments--;
                }
            }
            else
            {
                if (@interface == typeof(IRequestMiddleware))
                {
                    Add(MediatorRegistrationKind.RequestMiddleware, @interface, type);
                }
                else if (@interface == typeof(IStreamRequestMiddleware))
                {
                    Add(MediatorRegistrationKind.StreamRequestMiddleware, @interface, type);
                }
                else
                {
                    assignments--;
                }
            }


        }

        return assignments is 0;
    }
}
