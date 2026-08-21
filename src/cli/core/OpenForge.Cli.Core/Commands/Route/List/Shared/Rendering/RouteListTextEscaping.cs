namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;

internal static class RouteListTextEscaping
{
    internal const int ShortValueLimit = 160;
    internal const int DiagnosticValueLimit = 240;

    internal static string Escape(string? value)
    {
        return Escape(value, int.MaxValue);
    }

    internal static string Escape(string? value, int maximumLength)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (maximumLength < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumLength), maximumLength, "The text limit must be positive.");
        }

        var builder = new System.Text.StringBuilder(Math.Min(value.Length, maximumLength));
        var consumed = 0;
        foreach (var character in value)
        {
            var escaped = ReadEscaped(character);
            if (consumed + escaped.Length > maximumLength)
            {
                builder.Append("...");
                break;
            }

            builder.Append(escaped);
            consumed += escaped.Length;
        }

        return builder.ToString();
    }

    internal static string Clamp(string? value, int maximumLength)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (maximumLength < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumLength), maximumLength, "The text limit must be positive.");
        }

        return value.Length <= maximumLength
            ? value
            : value[..maximumLength] + "...";
    }

    private static string ReadEscaped(char character)
    {
        return character switch
        {
            '\\' => "\\\\",
            '"' => "\\\"",
            _ when char.IsControl(character) => $"\\u{(int)character:x4}",
            _ => character.ToString(),
        };
    }
}
