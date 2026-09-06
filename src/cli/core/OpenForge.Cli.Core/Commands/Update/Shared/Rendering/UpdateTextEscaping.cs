using System.Globalization;
using System.Text;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Rendering;

internal static class UpdateTextEscaping
{
    private const string Ellipsis = "...";

    internal const int DiagnosticValueLimit = 240;

    internal static string Escape(string value)
    {
        return EscapeValue(value);
    }

    internal static string Escape(string value, int maximumLength)
    {
        if (maximumLength < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumLength),
                maximumLength,
                "The text limit must be positive.");
        }

        var escaped = EscapeValue(value);
        if (escaped.Length <= maximumLength)
        {
            return escaped;
        }

        if (maximumLength <= Ellipsis.Length)
        {
            return new string('.', maximumLength);
        }

        var builder = new StringBuilder(maximumLength);
        var contentLength = maximumLength - Ellipsis.Length;
        for (var index = 0; index < value.Length; index++)
        {
            var token = ReadEscaped(value, ref index);
            if (builder.Length + token.Length > contentLength)
            {
                break;
            }

            builder.Append(token);
        }

        return builder.Append(Ellipsis).ToString();
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
                => string.Create(CultureInfo.InvariantCulture, $"\\u{(int)character:x4}"),
            _ => character.ToString(),
        };
    }
}
