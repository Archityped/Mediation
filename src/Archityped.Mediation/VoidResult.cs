namespace Archityped.Mediation;

/// <summary>
/// Represents an empty result used to model the absence of a value in generic contexts.
/// </summary>
/// <remarks>
/// Use <see cref="VoidResult"/> when a type argument is required but no value is produced.
/// This <see langword="struct"/> has no state; all instances are indistinguishable.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Advanced)]
public struct VoidResult
{
}