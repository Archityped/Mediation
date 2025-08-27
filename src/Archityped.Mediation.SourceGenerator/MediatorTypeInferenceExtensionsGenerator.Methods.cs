namespace Archityped.Mediation.SourceGenerator;

public sealed partial class MediatorTypeInferenceExtensionsGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Creates a method declaration syntax for a strongly-typed extension method.
    /// </summary>
    /// <param name="requestTypeName">The name of the request type.</param>
    /// <param name="responseTypeName">The name of the response type.</param>
    /// <param name="IsStreamType">A value that indicates whether this is a streaming request method.</param>
    /// <param name="isInternal">True if the request type is internal, otherwise false.</param>
    /// <returns>
    /// A <see cref="MethodDeclarationSyntax"/> representing either a <c>SendAsync</c> or <c>StreamAsync</c> extension method
    /// with strongly-typed parameters and return type.
    /// </returns>
    private static MethodDeclarationSyntax CreateMethodDeclaration(string requestTypeName, string responseTypeName, bool IsStreamType, bool isInternal)
    {
        var (methodName, returnType, inheritDocCref) = IsStreamType
            ? ("StreamAsync",
               $"IAsyncEnumerable<{responseTypeName}>",
               "IRequestSender.StreamAsync{TRequest,TResponse}(TRequest, CancellationToken)")
            : ("SendAsync",
               $"Task<{responseTypeName}>",
               "IRequestSender.SendAsync{TRequest,TResponse}(TRequest, CancellationToken)");

        return MethodDeclaration(ParseTypeName(returnType), Identifier(methodName))
            .WithAttributeLists(CreateMethodAttributes())
            .WithModifiers(CreateMethodModifiers(isInternal))
            .WithParameterList(CreateMethodParameterList(requestTypeName))
            .WithExpressionBody(CreateMethodBody(methodName, requestTypeName, responseTypeName))
            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            .WithLeadingTrivia(ParseLeadingTrivia($"/// <inheritdoc cref=\"{inheritDocCref}\" />\r\n"));
    }

    /// <summary>
    /// Creates attribute lists for generated extension methods.
    /// </summary>
    /// <returns>
    /// A <see cref="SyntaxList{T}"/> of <see cref="AttributeListSyntax"/> containing compiler-generated,
    /// editor browsability, and method implementation attributes.
    /// </returns>
    private static SyntaxList<AttributeListSyntax> CreateMethodAttributes() =>
        [AttributeList([
            Attribute(ParseName("CompilerGenerated")),
            Attribute(ParseName("EditorBrowsable"),
                AttributeArgumentList([
                    AttributeArgument(MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("EditorBrowsableState"),
                        IdentifierName("Never")))
                ])),
            Attribute(ParseName("MethodImpl"),
                AttributeArgumentList([
                    AttributeArgument(MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("MethodImplOptions"),
                        IdentifierName("AggressiveInlining")))
                ]))
        ])];

    /// <summary>
    /// Creates modifiers for generated extension methods.
    /// </summary>
    /// <param name="isInternal">True if the method should be internal, otherwise public.</param>
    /// <returns>A <see cref="SyntaxTokenList"/> containing accessibility and static modifiers.</returns>
    private static SyntaxTokenList CreateMethodModifiers(bool isInternal)
        => TokenList(isInternal ? Token(SyntaxKind.InternalKeyword) : Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword));

    /// <summary>
    /// Creates a parameter list for generated extension methods.
    /// </summary>
    /// <param name="requestTypeName">The name of the request type for the request parameter.</param>
    /// <returns>
    /// A <see cref="ParameterListSyntax"/> containing mediator (with <see langword="this"/> modifier),
    /// request, and optional cancellation token parameters.
    /// </returns>
    private static ParameterListSyntax CreateMethodParameterList(string requestTypeName) => ParameterList([
        Parameter(Identifier("mediator"))
            .WithType(ParseTypeName("IMediator"))
            .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword))),
        Parameter(Identifier("request"))
            .WithType(ParseTypeName(requestTypeName)),
        Parameter(Identifier("cancellationToken"))
            .WithType(ParseTypeName("CancellationToken"))
            .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.DefaultLiteralExpression)))
    ]);

    /// <summary>
    /// Creates an arrow expression clause for the method body that delegates to the mediator.
    /// </summary>
    /// <param name="methodName">The method name to invoke on the mediator (SendAsync or StreamAsync).</param>
    /// <param name="requestTypeName">The name of the request type.</param>
    /// <param name="responseTypeName">The name of the response type.</param>
    /// <returns>
    /// An <see cref="ArrowExpressionClauseSyntax"/> that calls the corresponding generic method on the mediator
    /// with explicit type arguments.
    /// </returns>
    private static ArrowExpressionClauseSyntax CreateMethodBody(string methodName, string requestTypeName, string responseTypeName) =>
        ArrowExpressionClause(
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("mediator"),
                    GenericName(Identifier(methodName))
                        .WithTypeArgumentList(TypeArgumentList([
                            ParseTypeName(requestTypeName),
                            ParseTypeName(responseTypeName)
                        ]))
                ),
                ArgumentList([
                    Argument(IdentifierName("request")),
                    Argument(IdentifierName("cancellationToken"))
                ])
            )
        );
}
