using Archityped.Mediation.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Archityped.Mediation;

/// <summary>
/// Provides extension methods to register the Archityped mediator and its components with the dependency injection container.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers the mediator and its components with the service collection using the specified assemblies and configuration.
    /// </summary>
    /// <param name="serviceDescriptors">The <see cref="IServiceCollection"/> to add the mediator services to.</param>
    /// <param name="assemblies">The assemblies to scan for handlers and middleware.</param>
    /// <param name="configuration">An action to configure the mediator registration.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceDescriptors"/>, <paramref name="assemblies"/>, or <paramref name="configuration"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method scans the provided assemblies for event handlers, request handlers, and middleware components that implement
    /// the appropriate interfaces, then applies the configuration action, and registers all components with the dependency injection container.
    /// All handlers and middleware are registered with <see cref="ServiceLifetime.Scoped"/> lifetime.
    /// </remarks>
#if NET8_0_OR_GREATER
    [RequiresUnreferencedCode(MethodRequiresDynamicCode)]
#endif
    public static IServiceCollection AddMediator(this IServiceCollection serviceDescriptors,
        IEnumerable<Assembly> assemblies, Action<MediatorConfiguration> configuration)
        => serviceDescriptors.AddMediatorCore(
            assemblies ?? throw new ArgumentNullException(nameof(configuration)),
            configuration ?? throw new ArgumentNullException(nameof(configuration)));

    /// <summary>
    /// Registers the mediator and its components with the service collection using the calling assembly.
    /// </summary>
    /// <param name="serviceDescriptors">The <see cref="IServiceCollection"/> to add the mediator services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceDescriptors"/> is <see langword="null"/>.</exception>
#if NET8_0_OR_GREATER
    [RequiresUnreferencedCode(MethodRequiresDynamicCode)]
#endif
    public static IServiceCollection AddMediator(this IServiceCollection serviceDescriptors)
        => serviceDescriptors.AddMediatorCore([Assembly.GetCallingAssembly()], null);

    /// <summary>
    /// Registers the mediator and its components with the service collection using the specified assemblies.
    /// </summary>
    /// <param name="serviceDescriptors">The <see cref="IServiceCollection"/> to add the mediator services to.</param>
    /// <param name="assemblies">The assemblies to scan for handlers and middleware.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceDescriptors"/> is <see langword="null"/>.</exception>
#if NET8_0_OR_GREATER
    [RequiresUnreferencedCode(MethodRequiresDynamicCode)]
#endif
    public static IServiceCollection AddMediator(this IServiceCollection serviceDescriptors, params Assembly[] assemblies)
        => serviceDescriptors.AddMediatorCore(
            assemblies ?? throw new ArgumentNullException(nameof(assemblies)),
            null);

    /// <summary>
    /// Registers the mediator and its components with the service collection using custom configuration.
    /// </summary>
    /// <param name="serviceDescriptors">The <see cref="IServiceCollection"/> to add the mediator services to.</param>
    /// <param name="configuration">An action to configure the mediator registration. If <see langword="null"/>, scans the calling assembly; otherwise, relies on manual configuration.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceDescriptors"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is <see langword="null"/>.</exception>"
    public static IServiceCollection AddMediator(this IServiceCollection serviceDescriptors, Action<MediatorConfiguration> configuration)
        => serviceDescriptors.AddMediatorCore(
            Enumerable.Empty<Assembly>(),
            configuration ?? throw new ArgumentNullException(nameof(configuration)));


    /// <summary>
    /// Registers the mediator and its components with the service collection using the specified assemblies and configuration.
    /// </summary>
    /// <param name="serviceDescriptors">The <see cref="IServiceCollection"/> to add the mediator services to.</param>
    /// <param name="assemblies">The assemblies to scan for handlers and middleware.</param>
    /// <param name="configuration">An optional action to configure the mediator registration.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceDescriptors"/> is <see langword="null"/>.</exception>
    private static IServiceCollection AddMediatorCore(this IServiceCollection serviceDescriptors, IEnumerable<Assembly>? assemblies, Action<MediatorConfiguration>? configuration)
    {
        var config = new MediatorConfiguration();

        if (assemblies is not null)
        {
            foreach (var assembly in assemblies)
            {
                config.Add(assembly);
            }
        }

        if (configuration is not null)
        {
            configuration(config);
            Debug.WriteIf(
                !config.Registrations.Any(),
                assemblies is not null && assemblies.Any()
                    ? $"Warning: No handlers or middleware were registered after scanning the following assemblies: {string.Join(", ", assemblies.Select(a => a.GetName().Name))}."
                    : "Warning: No handlers or middleware were registered in the mediator configuration.",
                "Archityped.Mediation");
        }

        RegisterServices(serviceDescriptors, config);
        return serviceDescriptors;
    }

    /// <summary>
    /// Registers all mediator services from the configuration with the service collection.
    /// </summary>
    /// <param name="serviceDescriptors">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="config">The mediator configuration containing the registrations.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers all handlers and middleware with <see cref="ServiceLifetime.Scoped"/> lifetime,
    /// and registers the <see cref="IMediator"/> service with the lifetime specified in the configuration.
    /// </remarks>
    private static IServiceCollection RegisterServices(IServiceCollection serviceDescriptors, MediatorConfiguration config)
    {
        foreach (var reg in config.Registrations)
        {
            if (reg.Factory is not null)
            {
                serviceDescriptors.AddSingleton(reg.ServiceType, reg.Factory);
            }
            else
            {
                serviceDescriptors.AddSingleton(reg.ServiceType, reg.ImplementationType);
            }
        }

        serviceDescriptors.Add(new ServiceDescriptor(typeof(IMediator), typeof(Mediator), config.ServiceLifetime));
        return serviceDescriptors;
    }
}
