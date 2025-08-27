namespace Archityped.Mediation.SourceGenerator;

public sealed partial class MediatorTypeInferenceExtensionsGenerator : IIncrementalGenerator
{
    private const string ClassName = "MediatorTypeInferenceExtensions";

    /// <summary>
    /// Creates the extension class declaration with the specified methods.
    /// </summary>
    /// <param name="methods">The collection of extension methods to include in the class.</param>
    /// <param name="isInternal">True if the class should be internal, otherwise public.</param>
    /// <returns>
    /// A <see cref="ClassDeclarationSyntax"/> representing a public or internal static partial class containing the specified methods.
    /// </returns>
    private static ClassDeclarationSyntax CreateClassDeclaration(IEnumerable<MemberDeclarationSyntax> methods, bool isInternal) =>
        ClassDeclaration($"__{ClassName}__")
            .WithModifiers(TokenList(
                isInternal ? Token(SyntaxKind.InternalKeyword) : Token(SyntaxKind.PublicKeyword),
                Token(SyntaxKind.StaticKeyword),
                Token(SyntaxKind.PartialKeyword)))
            .WithAttributeLists(CreateClassAttributes())
            .WithMembers(List(methods));

    /// <summary>
    /// Creates a namespace declaration containing the specified class declaration.
    /// </summary>
    /// <param name="classNamespace">The namespace name. If <see langword="null"/> or whitespace, defaults to "Archityped.Mediation".</param>
    /// <param name="classDecl">The class declaration to include in the namespace.</param>
    /// <returns>A <see cref="NamespaceDeclarationSyntax"/> containing the specified class declaration.</returns>
    private static NamespaceDeclarationSyntax CreateNamespaceDeclaration(string classNamespace, ClassDeclarationSyntax classDecl) =>
        NamespaceDeclaration(ParseName(string.IsNullOrWhiteSpace(classNamespace) ? "Archityped.Mediation" : classNamespace))
            .WithMembers(SingletonList<MemberDeclarationSyntax>(classDecl));

    /// <summary>
    /// Creates attribute lists for the generated extension class.
    /// </summary>
    /// <returns>
    /// A <see cref="SyntaxList{T}"/> of <see cref="AttributeListSyntax"/> containing compiler-generated,
    /// debugger, and editor browsability attributes.
    /// </returns>
    private static SyntaxList<AttributeListSyntax> CreateClassAttributes() =>
        [AttributeList([
            Attribute(IdentifierName("GeneratedCode"),
                AttributeArgumentList([
                    AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(nameof(MediatorTypeInferenceExtensionsGenerator)))),
                    AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(Version ?? "1.0.0.0")))
                ]))
            ]),
        AttributeList([
            Attribute(IdentifierName("CompilerGenerated")),
            Attribute(IdentifierName("DebuggerNonUserCode")),
            Attribute(IdentifierName("DebuggerStepThroughAttribute")),
            Attribute(IdentifierName("EditorBrowsable"),
                AttributeArgumentList([
                    AttributeArgument(MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("EditorBrowsableState"),
                        IdentifierName("Never")))
                ]))
        ])];
}
