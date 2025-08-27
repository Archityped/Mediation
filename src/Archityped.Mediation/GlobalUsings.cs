global using System.Diagnostics.CodeAnalysis;
global using System.ComponentModel;

#if NET8_0_OR_GREATER
global using static System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes;
global using static CompilerWarnings;

internal class CompilerWarnings
{
    internal const string MethodRequiresDynamicCode = "This method uses reflection to scan assemblies for handlers and middleware.";
}
#endif

