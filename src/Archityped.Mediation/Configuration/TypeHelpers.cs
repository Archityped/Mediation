namespace Archityped.Mediation.Configuration;

/// <summary>
/// Provides helper methods for type inspection and interface matching in the mediator configuration system.
/// </summary>
internal static class TypeHelpers
{
    /// <summary>
    /// Attempts to find a closed generic interface that matches the specified target type.
    /// </summary>
    /// <param name="currentType">The type to search for matching interfaces.</param>
    /// <param name="targetType">The target interface type to match against.</param>
    /// <param name="result">When this method returns <see langword="true"/>, contains the matching closed interface type; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if a matching closed interface is found; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method searches for interfaces that are either directly assignable from the target type
    /// or are closed generic types constructed from the same generic type definition.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFindClosedInterface(
#if NET6_0_OR_GREATER
        [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
        this Type currentType,
        Type targetType,
        [NotNullWhen(true)] out Type? result)
    {
        ReadOnlySpan<Type> span =
#if NET6_0_OR_GREATER
            MemoryMarshal.CreateReadOnlySpan(ref targetType, 1);
#else
            [targetType];
#endif
        return TryFindClosedInterface(currentType, span, out result);
    }

    /// <summary>
    /// Attempts to find a closed generic interface that matches any of the specified target types.
    /// </summary>
    /// <param name="currentType">The type to search for matching interfaces.</param>
    /// <param name="targetTypes">A span of target interface types to match against.</param>
    /// <param name="result">When this method returns <see langword="true"/>, contains the matching closed interface type; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if a matching closed interface is found; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// This method performs a comprehensive search for matching interfaces in the following order:
    /// </para>
    /// <list type="number">
    /// <item><description>Direct assignability check for non-generic target types</description></item>
    /// <item><description>Generic type definition matching for the current type if it's an interface</description></item>
    /// <item><description>Interface implementation scanning for all implemented interfaces</description></item>
    /// </list>
    /// <para>
    /// The method returns the first matching interface found during the search process.
    /// Generic type definitions are not supported as the current type parameter.
    /// </para>
    /// </remarks>
    public static bool TryFindClosedInterface(
#if NET6_0_OR_GREATER
        [DynamicallyAccessedMembers(PublicConstructors | Interfaces)]
#endif
           this Type currentType,
           ReadOnlySpan<Type> targetTypes,
           [NotNullWhen(true)] out Type? result)
    {
        Type definition, target;

        if (currentType.IsGenericTypeDefinition)
        {
            result = null;
            return false;
        }

        definition = currentType.IsGenericType 
            ? currentType.GetGenericTypeDefinition() 
            : currentType;

        // Scan closed targets
        for (int i = 0; i < targetTypes.Length; i++)
        {
            target = targetTypes[i];
            if (!target.IsGenericTypeDefinition && target.IsAssignableFrom(definition))
            {
                result = target;
                return true;
            }
        }

        // Scan closed interface, e.g., IEnumerable<string>
        if (currentType.IsInterface && currentType.IsConstructedGenericType)
        {
            definition = currentType.GetGenericTypeDefinition();
            for (int i = 0; i < targetTypes.Length; i++)
            {
                target = targetTypes[i];
                if (target.IsGenericTypeDefinition && (definition == target || target.IsAssignableFrom(definition)))
                {
                    result = currentType;
                    return true;
                }
            }
        }

        // Scan implemented interfaces
        var interfaces = currentType.GetInterfaces();
        for (int i = 0; i < interfaces.Length; i++)
        {
            target = interfaces[i];
            if (!target.IsGenericType) continue;

            definition = target.GetGenericTypeDefinition();
            for (int t = 0; t < targetTypes.Length; t++)
            {
                var open = targetTypes[t];
                if (open.IsGenericTypeDefinition && (definition == open || open.IsAssignableFrom(definition)))
                {
                    result = target;
                    return true;
                }
            }
        }

        result = null;
        return false;
    }
}
