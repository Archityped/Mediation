using Archityped.Mediation.Configuration;

namespace Archityped.Mediation;

/// <summary>
/// Provides extension methods to register the Archityped mediator and its components with the dependency injection container.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds mediator services to the dependency injection container using the specified configuration.
    /// </summary>
    /// <param name="serviceDescriptors">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">
    /// An optional action to configure mediator services. If <see langword="null"/>, the calling assembly will be scanned for handlers.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="serviceDescriptors"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// When no configuration is provided, the method automatically scans the calling assembly using <see cref="MediatorConfiguration.AddServicesFromAssembly"/>.
    /// Debug warnings are emitted when no services are registered to help identify configuration issues.
    /// </remarks>
    public static IServiceCollection AddMediator(this IServiceCollection serviceDescriptors, Action<MediatorConfiguration>? configuration)
    {
        var config = new MediatorConfiguration();

        if (configuration is not null)
        {
            configuration(config);
            Debug.WriteLineIf(
                config.ServiceRegistrations.Count is 0,
                "Warning: No mediator services were registered by the provided configuration. Verify that your configuration action adds handlers and middleware, or use AddServicesFromAssembly to scan the calling assembly.",
                "Archityped.Mediation");
        }
        else
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            config.AddServicesFromAssembly(callingAssembly);

            Debug.WriteLineIf(
                config.ServiceRegistrations.Count is 0,
                "Warning: No mediator services were registered. By default, the mediator scans the calling assembly using AddServicesFromAssembly. Ensure that your handlers and middleware are located in the calling assembly or provide a custom configuration action.",
                "Archityped.Mediation");

        }

        AddMediator(serviceDescriptors, config);
        return serviceDescriptors;
    }

    /// <summary>
    /// Adds mediator services to the dependency injection container using the specified configuration instance.
    /// </summary>
    /// <param name="serviceDescriptors">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The <see cref="MediatorConfiguration"/> containing the service registrations.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="serviceDescriptors"/> or <paramref name="configuration"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method registers all configured services and automatically adds default implementations for missing core services.
    /// A default <see cref="NotificationPublisher"/> is registered if no custom notification publisher is configured.
    /// Processor middleware is automatically registered when corresponding processors are detected.
    /// The lifetime of the notification publisher is adjusted to match the configured mediator lifetime if necessary.
    /// </remarks>
    public static IServiceCollection AddMediator(this IServiceCollection serviceDescriptors, MediatorConfiguration configuration)
    {
        if (serviceDescriptors is null)
            throw new ArgumentNullException(nameof(serviceDescriptors));

        if (configuration is null)
            throw new ArgumentNullException(nameof(configuration));

        var notificationPublisherDescriptor = GetNotificationPublisherDescriptor(configuration);

        serviceDescriptors.TryAdd(notificationPublisherDescriptor);
        serviceDescriptors.TryAdd(new ServiceDescriptor(typeof(IMediator), typeof(Mediator), configuration.Lifetime));
        serviceDescriptors.TryAdd(new ServiceDescriptor(typeof(IRequestSender), sp => sp.GetRequiredService<IMediator>(), configuration.Lifetime));

        RegisterCoreServices(serviceDescriptors, configuration);

        return serviceDescriptors;
    }

    /// <summary>
    /// Registers core mediator services from the configuration and automatically adds processor middleware wrappers.
    /// </summary>
    /// <param name="serviceDescriptors">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The <see cref="MediatorConfiguration"/> containing the service registrations.</param>
    /// <remarks>
    /// This method automatically registers middleware wrappers for processors: <see cref="RequestPreProcessor"/> and 
    /// <see cref="RequestPostProcessor"/> for request processors, and <see cref="StreamRequestPreProcessor"/> for stream request processors.
    /// Services with implementation types are added as enumerable services to support multiple registrations.
    /// Services without implementation types (factory-based) are added as single registrations.
    /// </remarks>
    private static void RegisterCoreServices(IServiceCollection serviceDescriptors, MediatorConfiguration configuration)
    {
        MediatorComponentType componentType;

        foreach (var descriptor in configuration.ServiceRegistrations)
        {
            componentType = descriptor.ComponentType;

            if (componentType is MediatorComponentType.RequestProcessor)
            {
                if (descriptor.ServiceType == typeof(IRequestPreProcessor))
                {
                    serviceDescriptors.TryAddEnumerable(new ServiceDescriptor(typeof(IRequestMiddleware), typeof(RequestPreProcessor), ServiceLifetime.Transient));
                }
                else if (descriptor.ServiceType == typeof(IRequestPostProcessor))
                {
                    serviceDescriptors.TryAddEnumerable(new ServiceDescriptor(typeof(IRequestMiddleware), typeof(RequestPostProcessor), ServiceLifetime.Transient));
                }
            }
            else if (componentType is MediatorComponentType.StreamRequestProcessor)
            {
                serviceDescriptors.TryAddEnumerable(new ServiceDescriptor(typeof(IStreamRequestMiddleware), typeof(StreamRequestPreProcessor), ServiceLifetime.Transient));
            }

            if (descriptor.ImplementationType is null)
            {
                serviceDescriptors.Add(descriptor);
            }
            else
            {
                serviceDescriptors.TryAddEnumerable(descriptor);
            }
        }
    }

    /// <summary>
    /// Gets the notification publisher service descriptor, ensuring the lifetime matches the configuration.
    /// </summary>
    /// <param name="configuration">The <see cref="MediatorConfiguration"/> containing the notification publisher registration.</param>
    /// <returns>A <see cref="ServiceDescriptor"/> for the notification publisher with the correct lifetime.</returns>
    /// <remarks>
    /// If no custom notification publisher is configured, a default <see cref="NotificationPublisher"/> is used.
    /// If a custom publisher is configured but has a different lifetime than the mediator configuration, 
    /// a new descriptor is created with the matching lifetime.
    /// </remarks>
    private static ServiceDescriptor GetNotificationPublisherDescriptor(MediatorConfiguration configuration)
        => configuration.NotificationPublisherRegistration is { } notificationPublisherDescriptor
            ? notificationPublisherDescriptor.Lifetime == configuration.Lifetime
                ? notificationPublisherDescriptor
                : notificationPublisherDescriptor.ImplementationType is not null
                    ? new ServiceDescriptor(notificationPublisherDescriptor.ServiceType, notificationPublisherDescriptor.ImplementationType, configuration.Lifetime)
                    : new ServiceDescriptor(notificationPublisherDescriptor.ServiceType, notificationPublisherDescriptor.ImplementationFactory!, configuration.Lifetime)
            : new ServiceDescriptor(typeof(INotificationPublisher), typeof(NotificationPublisher), configuration.Lifetime);

}
