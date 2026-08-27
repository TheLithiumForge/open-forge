namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;

internal static class RouteListTextEscaping
{
    private const string TruncationMarker = "...";

    internal const int ShortValueLimit = 160;
    internal const int DiagnosticValueLimit = 240;

    internal static string Escape(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return BuildEscaped(value);
    }

    internal static string Escape(string value, int maximumLength)
    {
        ArgumentNullException.ThrowIfNull(value);
        ValidateMaximumLength(maximumLength);

        var escapedValue = BuildEscaped(value);
        if (escapedValue.Length <= maximumLength)
        {
            return escapedValue;
        }

        if (maximumLength <= TruncationMarker.Length)
        {
            return new string('.', maximumLength);
        }

        var contentLength = maximumLength - TruncationMarker.Length;
        var builder = new System.Text.StringBuilder(Math.Min(value.Length, contentLength));
        for (var index = 0; index < value.Length; index++)
        {
            var escapedToken = ReadEscaped(value, ref index);
            if (escapedToken.Length > contentLength - builder.Length)
            {
                break;
            }

            builder.Append(escapedToken);
        }

        builder.Append(TruncationMarker);
        return builder.ToString();
    }

    internal static string Clamp(string value, int maximumLength)
    {
        ArgumentNullException.ThrowIfNull(value);
        ValidateMaximumLength(maximumLength);

        if (value.Length <= maximumLength)
        {
            return value;
        }

        if (maximumLength <= TruncationMarker.Length)
        {
            return new string('.', maximumLength);
        }

        var contentLength = maximumLength - TruncationMarker.Length;
        var index = 0;
        while (index < value.Length)
        {
            var scalarLength = ReadScalarLength(value, index);
            if (scalarLength > contentLength - index)
            {
                break;
            }

            index += scalarLength;
        }

        return string.Concat(value.AsSpan(0, index), TruncationMarker.AsSpan());
    }

    private static string BuildEscaped(string value)
    {
        var builder = new System.Text.StringBuilder(value.Length);
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
            _ when char.IsControl(character) || char.IsSurrogate(character) => $"\\u{(int)character:x4}",
            _ => character.ToString(),
        };
    }

    private static int ReadScalarLength(string value, int index)
    {
        return char.IsHighSurrogate(value[index])
            && index + 1 < value.Length
            && char.IsLowSurrogate(value[index + 1])
            ? 2
            : 1;
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
