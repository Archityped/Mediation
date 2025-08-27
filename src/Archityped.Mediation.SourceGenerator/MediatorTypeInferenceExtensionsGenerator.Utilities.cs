namespace Archityped.Mediation.SourceGenerator;

public sealed partial class MediatorTypeInferenceExtensionsGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Sanitizes the specified input string by replacing invalid characters with valid identifier characters.
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <returns>A <see cref="string"/> containing only valid identifier characters, with symbols replaced by underscores.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    private static string Sanitize(string input) => Sanitize(input.AsSpan());

    /// <summary>
    /// Sanitizes the specified input span by replacing invalid characters with valid identifier characters.
    /// </summary>
    /// <param name="input">The input character span to sanitize.</param>
    /// <returns>A <see cref="string"/> containing only valid identifier characters, with symbols replaced by underscores.</returns>
    private unsafe static string Sanitize(ReadOnlySpan<char> input)
    {
        var length = input.Length;
        var temp = input.Length <= 256 ? stackalloc char[length] : new char[length];
        int pos = 0;

        for (var index = 0; index < length; index++)
        {
            var currentChar = input[index];
            if (char.IsLetterOrDigit(currentChar) || currentChar is '.' or '_')
            {
                temp[pos++] = currentChar;
                continue;
            }

            if (char.IsSymbol(currentChar))
            {
                temp[pos++] = '_';
                continue;
            }
        }

        fixed (char* p = temp)
        {
            return new string(p, 0, pos);
        }
    }
}