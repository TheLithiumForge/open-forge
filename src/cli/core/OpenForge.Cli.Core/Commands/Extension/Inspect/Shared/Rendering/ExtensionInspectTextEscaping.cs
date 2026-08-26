using System.Text;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;

internal static class ExtensionInspectTextEscaping
{
    internal const int DiagnosticValueLimit = 240;

    internal static string Escape(string value)
    {
        var builder = new StringBuilder(value.Length);
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsHighSurrogate(character)
                && index + 1 < value.Length
                && char.IsLowSurrogate(value[index + 1]))
            {
                builder.Append(character);
                builder.Append(value[++index]);
                continue;
            }

            builder.Append(character switch
            {
                '\\' => "\\\\",
                '"' => "\\\"",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\u0009",
                '\b' => "\\b",
                '\f' => "\\f",
                _ when char.IsControl(character) || char.IsSurrogate(character)
                    => $"\\u{(int)character:x4}",
                _ => character.ToString(),
            });
        }

        return builder.ToString();
    }

    internal static string Clamp(string value, int maximumLength)
    {
        if (maximumLength < 4)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumLength), maximumLength, "The text limit must be at least four.");
        }

        return value.Length <= maximumLength
            ? value
            : value[..(maximumLength - 3)] + "...";
    }
}
