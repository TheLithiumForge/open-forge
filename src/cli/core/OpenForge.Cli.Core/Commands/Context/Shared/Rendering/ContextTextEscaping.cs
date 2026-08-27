using System.Text;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Rendering;

internal static class ContextTextEscaping
{
    internal static string Escape(string value, int maximumLength)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (maximumLength < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumLength),
                maximumLength,
                "The text limit must be positive.");
        }

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
        var builder = new StringBuilder(maximumLength);
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
        var builder = new StringBuilder(value.Length);
        for (var index = 0; index < value.Length; index++)
        {
            builder.Append(ReadEscaped(value, ref index));
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
}
