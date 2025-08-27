namespace Archityped.Mediation.Configuration;

public partial class MediatorConfiguration
{
    /// <summary>
    /// Adds mediator services from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">A type whose assembly will be scanned for mediator services.</typeparam>
    /// <remarks>
    /// This method only scans for types implementing mediator handler interfaces such as <see cref="INotificationHandler{TNotification}"/>, 
    /// <see cref="IRequestHandler{TRequest}"/>, <see cref="IRequestHandler{TRequest, TResponse}"/>, and <see cref="IStreamRequestHandler{TRequest, TResponse}"/>.
    /// </remarks>
    public void AddServicesFromAssemblyOf<T>()
        => AddServicesFromAssembly(typeof(T).Assembly);

    /// <summary>
    /// Adds mediator services from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to scan for mediator services.</param>
    /// <remarks>
    /// This method only scans for types implementing mediator handler interfaces such as <see cref="INotificationHandler{TNotification}"/>, 
    /// <see cref="IRequestHandler{TRequest}"/>, <see cref="IRequestHandler{TRequest, TResponse}"/>, and <see cref="IStreamRequestHandler{TRequest, TResponse}"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
    public void AddServicesFromAssembly(Assembly assembly) => _serviceDescriptors.AddRange(FindServiceDescriptors(assembly));

    /// <summary>
    /// Adds mediator services from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan for mediator services.</param>
    /// <remarks>
    /// This method only scans for types implementing mediator handler interfaces such as <see cref="INotificationHandler{TNotification}"/>, 
    /// <see cref="IRequestHandler{TRequest}"/>, <see cref="IRequestHandler{TRequest, TResponse}"/>, and <see cref="IStreamRequestHandler{TRequest, TResponse}"/>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddServicesFromAssemblies(params Assembly[] assemblies)
        => AddServicesFromAssemblies(assemblies?.AsEnumerable()!);

    /// <summary>
    /// Adds mediator services from the specified collection of assemblies.
    /// </summary>
    /// <param name="assemblies">The collection of assemblies to scan for mediator services.</param>
    /// <remarks>
    /// This method only scans for types implementing mediator handler interfaces such as <see cref="INotificationHandler{TNotification}"/>, 
    /// <see cref="IRequestHandler{TRequest}"/>, <see cref="IRequestHandler{TRequest, TResponse}"/>, and <see cref="IStreamRequestHandler{TRequest, TResponse}"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="assemblies"/> is <see langword="null"/>.</exception>
    public void AddServicesFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        if (assemblies is null)
            throw new ArgumentNullException(nameof(assemblies));

        var descriptors = assemblies.SelectMany(FindServiceDescriptors);
        _serviceDescriptors.AddRange(descriptors);
    }

    /// <summary>
    /// Finds mediator service descriptors in the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to scan for mediator services.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="MediatorServiceDescriptor"/> instances representing the discovered services.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
    private static IEnumerable<MediatorServiceDescriptor> FindServiceDescriptors(Assembly assembly)
        => assembly?.GetTypes().SelectMany(FindServiceDescriptors)
        ?? throw new ArgumentNullException(nameof(assembly));

    /// <summary>
    /// Finds mediator service descriptors for the specified type.
    /// </summary>
    /// <param name="type">The type to examine for mediator interface implementations.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="MediatorServiceDescriptor"/> instances for each mediator interface implemented by the type.</returns>
    /// <remarks>
    private static IEnumerable<MediatorServiceDescriptor> FindServiceDescriptors(Type type)
    {
        if (type.IsInterface | type.IsAbstract)
        {
            yield break;
        }

        var interfaces = type.GetInterfaces();

        foreach (var @interface in interfaces)
        {
            if (@interface.IsGenericType)
            {
                var genericType = @interface.GetGenericTypeDefinition();
                if (genericType == typeof(INotificationHandler<>))
                {
                    yield return new(MediatorComponentType.NotificationHandler, @interface, type);
                }
                else if (genericType == typeof(IRequestHandler<>))
                {
                    yield return new(MediatorComponentType.RequestHandler, @interface, type);
                }
                else if (genericType == typeof(IRequestHandler<,>))
                {
                    yield return new(MediatorComponentType.RequestHandler, @interface, type);
                }
                else if (genericType == typeof(IStreamRequestHandler<,>))
                {
                    yield return new(MediatorComponentType.StreamRequestHandler, @interface, type);
                }
                else if (genericType == typeof(IStreamRequest<>))
                {
                    yield return new(MediatorComponentType.StreamRequestProcessor, @interface, type);
                }
            }
        }
    }
}
