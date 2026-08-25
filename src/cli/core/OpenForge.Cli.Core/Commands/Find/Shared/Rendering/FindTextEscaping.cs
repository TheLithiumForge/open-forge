namespace OpenForge.Cli.Core.Commands.Find.Shared.Rendering;

internal static class FindTextEscaping
{
    internal const int DiagnosticValueLimit = 240;

    internal static string Escape(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return EscapeValue(value);
    }

    internal static string Escape(string value, int maximumLength)
    {
        ArgumentNullException.ThrowIfNull(value);
        ValidateMaximumLength(maximumLength);
        var escapedValue = EscapeValue(value);
        if (escapedValue.Length <= maximumLength)
        {
            return escapedValue;
        }

        if (maximumLength <= 3)
        {
            return new string('.', maximumLength);
        }

        var contentLength = maximumLength - 3;
        var builder = new System.Text.StringBuilder(maximumLength);
        for (var index = 0; index < value.Length; index++)
        {
            var escaped = ReadEscaped(value, ref index);
            if (escaped.Length > contentLength - builder.Length)
            {
                break;
            }

            builder.Append(escaped);
        }

        builder.Append("...");
        return builder.ToString();
    }

    private static string EscapeValue(string value)
    {
        var builder = new System.Text.StringBuilder(value.Length);
        for (var index = 0; index < value.Length; index++)
        {
            var escaped = ReadEscaped(value, ref index);
            builder.Append(escaped);
        }

        return builder.ToString();
    }

    private static string ReadEscaped(string value, ref int index)
    {
        var character = value[index];
        if (char.IsHighSurrogate(character)
            && index + 1 < value.Length
            && char.IsLowSurrogate(value[index + 1]))
        {
            index++;
            return string.Concat(character, value[index]);
        }

        return character switch
        {
            '\\' => "\\\\",
            '"' => "\\\"",
            _ when char.IsControl(character) || char.IsSurrogate(character)
                => $"\\u{(int)character:x4}",
            _ => character.ToString(),
        };
    }

    private static void ValidateMaximumLength(int maximumLength)
    {
        if (maximumLength < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumLength),
                maximumLength,
                "The text limit must be positive.");
        }
    }
}
