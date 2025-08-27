namespace Archityped.Mediation.SourceGenerator;

/// <summary>
/// Represents information about a request type for source generation.
/// </summary>
/// <param name="TRequest">The request type symbol.</param>
/// <param name="TResponse">The response type symbol.</param>
/// <param name="TargetNamespace">The target namespace for the generated extension methods, or <see langword="null"/> if no namespace is specified.</param>
/// <param name="IsStreamType">A value that indicates whether this represents a streaming request type.</param>
internal readonly record struct RequestInfo(ITypeSymbol TRequest, ITypeSymbol TResponse, string? TargetNamespace, bool IsStreamType);

/// <summary>
/// Represents an incremental source generator that creates type inference extension methods for mediator request/response patterns.
/// </summary>
/// <remarks>
/// <para>
/// This generator analyzes types implementing <c>IRequest{T}</c> and <c>IStreamRequest{T}</c> interfaces
/// and generates strongly-typed extension methods to improve type inference for generic mediator operations.
/// </para>
/// <para>
/// The generator creates extension methods that eliminate the need to explicitly specify generic type parameters
/// when calling mediator methods, improving developer experience and reducing boilerplate code.
/// </para>
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed partial class MediatorTypeInferenceExtensionsGenerator : IIncrementalGenerator
{
    private const string ITargetRequestFullName = "Archityped.Mediation.IRequest`1";
    private const string ITargetStreamRequestFullName = "Archityped.Mediation.IStreamRequest`1";
    private static string? Version;

    private static readonly SymbolDisplayFormat NestedNameFormat = new(
    globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
    genericsOptions:
        SymbolDisplayGenericsOptions.IncludeTypeParameters,
    miscellaneousOptions:
        SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
        SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);


    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var versionInfo = context.CompilationProvider.Select((compilation, _) => compilation.Assembly.Identity.Version?.ToString());
        var symbolProviders = CreateSymbolProviders(context);
        var syntaxProvider = CreateSyntaxProvider(context);
        var candidates = CombineCandidates(syntaxProvider, symbolProviders);
        var requestInfo = ExtractRequestInfo(candidates);

        RegisterSourceOutput(context, requestInfo, versionInfo);
    }

    /// <summary>
    /// Creates symbol providers for request and stream request interfaces.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    /// <returns>
    /// A tuple containing incremental value providers for <c>IRequest{T}</c> and <c>IStreamRequest{T}</c> symbols,
    /// or <see langword="null"/> if the symbols cannot be found in the compilation.
    /// </returns>
    private static (IncrementalValueProvider<INamedTypeSymbol?> Request, IncrementalValueProvider<INamedTypeSymbol?> StreamRequest) CreateSymbolProviders(IncrementalGeneratorInitializationContext context)
    {
        var iRequestSymbolProvider = context.CompilationProvider
            .Select(static (c, _) => c.GetTypeByMetadataName(ITargetRequestFullName));

        var iStreamRequestSymbolProvider = context.CompilationProvider
            .Select(static (c, _) => c.GetTypeByMetadataName(ITargetStreamRequestFullName));

        return (iRequestSymbolProvider, iStreamRequestSymbolProvider);
    }

    /// <summary>
    /// Recursively enumerates all type declarations (classes, structs, records) including nested types.
    /// </summary>
    /// <param name="root">The root syntax node to search.</param>
    /// <returns>An enumerable of all TypeDeclarationSyntax nodes found.</returns>
    private static IEnumerable<TypeDeclarationSyntax> GetAllTypeDeclarations(SyntaxNode root)
    {
        foreach (var node in root.DescendantNodesAndSelf())
        {
            if (node is TypeDeclarationSyntax t)
                yield return t;
        }
    }

    /// <summary>
    /// Creates a syntax provider that identifies type declarations with base lists, including nested types.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    /// <returns>
    /// An <see cref="IncrementalValuesProvider{T}"/> that provides tuples of SemanticModel and TypeDeclarationSyntax
    /// for type declarations that have base type or interface implementations, including nested types.
    /// </returns>
    private static IncrementalValuesProvider<(SemanticModel SemanticModel, TypeDeclarationSyntax Node)> CreateSyntaxProvider(IncrementalGeneratorInitializationContext context)
        => context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (s, _) =>
            {
                // Accept any SyntaxNode that is a CompilationUnit or NamespaceDeclaration
                // We'll enumerate all nested type declarations in transform
                return s is CompilationUnitSyntax || s is NamespaceDeclarationSyntax;
            },
            transform: static (ctx, _) => ctx
        )
        .SelectMany(static (ctx, _) =>
        {
            // Recursively enumerate all type declarations with base lists
            return GetAllTypeDeclarations(ctx.Node)
                .Where(t => t.BaseList is not null)
                .Select(t => (ctx.SemanticModel, t));
        });

    /// <summary>
    /// Combines syntax candidates with symbol providers to identify request types.
    /// </summary>
    /// <param name="syntaxProvider">The syntax provider for type declarations.</param>
    /// <param name="symbolProviders">The symbol providers for request interface types.</param>
    /// <returns>
    /// An <see cref="IncrementalValuesProvider{T}"/> containing semantic model, type declaration, request symbols, and stream type indicators
    /// for types that implement request interfaces.
    /// </returns>
    private static IncrementalValuesProvider<(SemanticModel SemanticModel, TypeDeclarationSyntax Node, INamedTypeSymbol RequestSymbol, bool IsStreamType)> CombineCandidates(
        IncrementalValuesProvider<(SemanticModel SemanticModel, TypeDeclarationSyntax Node)> syntaxProvider,
        (IncrementalValueProvider<INamedTypeSymbol?> Request, IncrementalValueProvider<INamedTypeSymbol?> StreamRequest) symbolProviders)
    {
        var requestMethodCandidates = syntaxProvider
            .Combine(symbolProviders.Request)
            .Where(static (pair) => pair.Right is not null)
            .Select(static (pair, _) => (pair.Left.SemanticModel, pair.Left.Node, pair.Right!, false));

        var streamRequestMethodCandidates = syntaxProvider
            .Combine(symbolProviders.StreamRequest)
            .Where(static (pair) => pair.Right is not null)
            .Select(static (pair, _) => (pair.Left.SemanticModel, pair.Left.Node, pair.Right!, true));

        return requestMethodCandidates
            .Collect()
            .Combine(streamRequestMethodCandidates.Collect())
            .SelectMany(static (pair, _) => pair.Left.Concat(pair.Right));
    }

    /// <summary>
    /// Extracts request information from candidate types.
    /// </summary>
    /// <param name="candidates">The collection of candidate syntax contexts and symbols.</param>
    /// <returns>
    /// An <see cref="IncrementalValuesProvider{T}"/> containing validated <see cref="RequestInfo"/> structures
    /// for types that represent valid request types.
    /// </returns>
    private static IncrementalValuesProvider<RequestInfo> ExtractRequestInfo(IncrementalValuesProvider<(SemanticModel SemanticModel, TypeDeclarationSyntax Node, INamedTypeSymbol RequestSymbol, bool IsStreamType)> candidates)
        => candidates
            .Select(static (candidate, _) => GetRequestInfo(candidate.SemanticModel, candidate.Node, candidate.RequestSymbol, candidate.IsStreamType))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!.Value);

    /// <summary>
    /// Registers source output for the collected request information.
    /// </summary>
    /// <param name="context">The generator initialization context.</param>
    /// <param name="requestInfo">The collection of validated request information.</param>
    /// <param name="version">The version string for the generated code.</param>
    /// <remarks>
    /// This method groups requests by namespace and generates separate source files for each namespace
    /// containing the appropriate extension methods.
    /// </remarks>
    private static void RegisterSourceOutput(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<RequestInfo> requestInfo, IncrementalValueProvider<string?> version)
    {
        context.RegisterSourceOutput(requestInfo.Collect().Combine(version), static (context, items) =>
        {
            var (requestInfoCollection, version) = items;
            Version = version;

            var grouped = requestInfoCollection
                .Distinct()
                .GroupBy(h => h.TargetNamespace ?? "")
                .ToDictionary(g => g.Key, g => g.Select(x => (x.TRequest, x.TResponse, x.IsStreamType)));

            foreach (var kvp in grouped)
            {
                var ns = kvp.Key;
                var pairs = kvp.Value;
                var fileName = $"{(string.IsNullOrWhiteSpace(ns) ? "Global" :Sanitize(ns.AsSpan()))}.{ClassName}.g.cs";
                var source = GenerateNamespaceExtensionFile(ns, pairs);
                context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
            }
        });
    }

    /// <summary>
    /// Extracts request information from a semantic model, type declaration, and request symbol.
    /// </summary>
    /// <param name="semanticModel">The semantic model for the syntax tree.</param>
    /// <param name="node">The type declaration syntax node.</param>
    /// <param name="requestSymbol">The symbol representing the request interface type.</param>
    /// <param name="IsStreamType">A value that indicates whether this is a streaming request type.</param>
    /// <returns>
    /// A <see cref="RequestInfo"/> structure containing the extracted type information,
    /// or <see langword="null"/> if the context does not represent a valid request type.
    /// </returns>
    private static RequestInfo? GetRequestInfo(SemanticModel semanticModel, TypeDeclarationSyntax node, INamedTypeSymbol requestSymbol, bool IsStreamType)
    {
        if (semanticModel.GetDeclaredSymbol(node) is not INamedTypeSymbol typeSymbol)
            return null;

        if (typeSymbol.IsAbstract || typeSymbol.TypeKind is not (TypeKind.Class or TypeKind.Struct))
            return null;

        foreach (var interfaceSymbol in typeSymbol.AllInterfaces)
        {
            if (!interfaceSymbol.IsGenericType || interfaceSymbol.TypeArguments.Length != 1)
                continue;

            if (!SymbolEqualityComparer.Default.Equals(interfaceSymbol.ConstructedFrom, requestSymbol))
                continue;

            var requestType = typeSymbol;
            var responseType = interfaceSymbol.TypeArguments[0];
            var targetNamespace = requestType.ContainingNamespace.IsGlobalNamespace ? null : requestType.ContainingNamespace?.ToDisplayString();
            return new(requestType, responseType, targetNamespace, IsStreamType);
        }

        return null;
    }

    private static bool IsEffectivelyInternal(INamedTypeSymbol typeSymbol)
    {
        var current = typeSymbol;
        while (current is not null)
        {
            if (current.DeclaredAccessibility is Accessibility.Internal)
                return true;

            current = current.ContainingType;
        }

        return false;
    }

    /// <summary>
    /// Generates a complete source file containing extension methods for a specific namespace.
    /// </summary>
    /// <param name="targetNamespace">The target namespace for the generated extension methods.</param>
    /// <param name="pairs">
    /// The collection of request/response type pairs to generate extension methods for,
    /// along with stream type indicators.
    /// </param>
    /// <returns>
    /// A <see cref="string"/> containing the complete source code for the extension class,
    /// including all necessary using directives, namespace declarations, and extension methods.
    /// </returns>
    /// <remarks>
    /// This method automatically determines required using directives based on the types involved
    /// and generates a compilation unit with proper formatting and auto-generated file markers.
    /// </remarks>
    private static string GenerateNamespaceExtensionFile(string targetNamespace, IEnumerable<(ITypeSymbol TRequest, ITypeSymbol TResponse, bool IsStreamType)> pairs)
    {
        var usings = new HashSet<string>()
        {
            "System.CodeDom.Compiler",
            "System.ComponentModel",
            "System.Diagnostics",
            "System.Runtime.CompilerServices",
            "System.Threading",
            "System.Threading.Tasks",
            "Archityped.Mediation"
        };

        void AddNamespaceToUsingsIfValid(INamespaceSymbol? namespaceSymbol)
        {
            if (namespaceSymbol is null || namespaceSymbol.IsGlobalNamespace)
                return;

            var ns = namespaceSymbol.ToDisplayString();
            if (!string.IsNullOrWhiteSpace(ns))
                usings.Add(ns);
        }

        var methods = new List<MemberDeclarationSyntax>();

        foreach (var (tRequest, tResponse, IsStreamType) in pairs)
        {
            var requestTypeName = tRequest.ToDisplayString(NestedNameFormat);
            var responseTypeName = tResponse.ToDisplayString(NestedNameFormat);

            AddNamespaceToUsingsIfValid(tRequest.ContainingNamespace);
            AddNamespaceToUsingsIfValid(tResponse.ContainingNamespace);

            if (tResponse is INamedTypeSymbol named && named.IsGenericType)
            {
                foreach (var arg in named.TypeArguments)
                    AddNamespaceToUsingsIfValid(arg.ContainingNamespace);
            }

            if (IsStreamType)
            {
                usings.Add("System.Collections.Generic");
            }

            bool methodIsInternal = IsEffectivelyInternal((INamedTypeSymbol)tRequest);
            var method = CreateMethodDeclaration(requestTypeName, responseTypeName, IsStreamType, methodIsInternal);
            methods.Add(method);
        }

        bool isInternal = methods.All(m => m.Modifiers.Any(SyntaxKind.InternalKeyword));
        return GenerateCompilationUnit(targetNamespace, usings, methods, isInternal);
    }

    /// <summary>
    /// Generates a complete compilation unit with the specified namespace, using directives, and methods.
    /// </summary>
    /// <param name="targetNamespace">The target namespace for the generated code.</param>
    /// <param name="usings">The collection of using directive namespaces to include.</param>
    /// <param name="methods">The collection of method declarations to include in the generated class.</param>
    /// <param name="isInternal">Indicates whether the generated class and methods should be internal.</param>
    /// <returns>
    /// A <see cref="string"/> containing the formatted source code for the complete compilation unit
    /// with auto-generated file markers and normalized whitespace.
    /// </returns>
    /// <remarks>
    /// The generated compilation unit includes proper auto-generated markers, sorted using directives,
    /// and normalized whitespace formatting for consistent output.
    /// </remarks>
    private static string GenerateCompilationUnit(string targetNamespace, IEnumerable<string> usings, IEnumerable<MemberDeclarationSyntax> methods, bool isInternal)
    {
        var unit = CompilationUnit()
                .WithUsings(List(usings
                    .OrderBy(u => u, StringComparer.OrdinalIgnoreCase)
                    .Select(u => UsingDirective(ParseName(u)))));

        var classDeclaration = CreateClassDeclaration(methods, isInternal);

        if (string.IsNullOrWhiteSpace(targetNamespace))
        {
            unit = unit.WithMembers(SingletonList<MemberDeclarationSyntax>(classDeclaration));
        }
        else
        {
            var namespaceDeclaration = CreateNamespaceDeclaration(targetNamespace, classDeclaration);
            unit = unit.WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration));
        }

        var normalizedUnit = unit.NormalizeWhitespace(elasticTrivia: true);
        var buffer = new StringBuilder();
        using var writer = new StringWriter(buffer);
        normalizedUnit.WriteTo(writer);
        return buffer.ToString();
    }
}
