namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectTextEscaping
{
    internal const int HumanValueLimit = 512;

    internal const int DiagnosticValueLimit = 240;

    internal const int DefaultValueLimit = 8192;

    internal static string Escape(string value)
    {
        return Escape(value, DefaultValueLimit);
    }

    internal static string Escape(string value, int maximumLength)
    {
        ValidateMaximumLength(maximumLength);

        var builder = new System.Text.StringBuilder(Math.Min(value.Length, maximumLength));
        for (var index = 0; index < value.Length; index++)
        {
            var escaped = ReadEscaped(value, ref index);
            if (escaped.Length > maximumLength - builder.Length)
            {
                AppendTruncation(builder, maximumLength);
                break;
            }

            builder.Append(escaped);
        }

        return builder.ToString();
    }

    internal static string Clamp(string value, int maximumLength)
    {
        ValidateMaximumLength(maximumLength);
        if (value.Length <= maximumLength)
        {
            return value;
        }

        if (maximumLength <= 3)
        {
            return new string('.', maximumLength);
        }

        var contentLength = maximumLength - 3;
        if (contentLength > 0
            && contentLength < value.Length
            && char.IsHighSurrogate(value[contentLength - 1]))
        {
            contentLength--;
        }

        return value[..contentLength] + "...";
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

        if (character is '\\' or '"')
        {
            return character == '\\' ? "\\\\" : "\\\"";
        }

        if (char.IsControl(character) || char.IsSurrogate(character))
        {
            return $"\\u{(int)character:x4}";
        }

        return character.ToString();
    }

    private static void AppendTruncation(
        System.Text.StringBuilder builder,
        int maximumLength)
    {
        if (maximumLength <= 3)
        {
            builder.Append('.', maximumLength - builder.Length);
            return;
        }

        var contentLength = maximumLength - 3;
        if (builder.Length > contentLength)
        {
            if (contentLength > 0 && char.IsHighSurrogate(builder[contentLength - 1]))
            {
                contentLength--;
            }

            builder.Length = contentLength;
        }

        builder.Append("...");
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
