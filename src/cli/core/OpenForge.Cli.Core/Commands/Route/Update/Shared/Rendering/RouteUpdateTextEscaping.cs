using System.Text.Json;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Rendering;

internal static class RouteUpdateTextEscaping
{
    private const string Ellipsis = "...";

    internal const int DiagnosticValueLimit = 240;

    internal static string Escape(string value)
        => JsonEncodedText.Encode(value).ToString();

    internal static string Escape(string value, int maximumLength)
    {
        if (maximumLength < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumLength),
                maximumLength,
                "The text limit must be positive.");
        }

        var escaped = Escape(value);
        if (escaped.Length <= maximumLength)
        {
            return escaped;
        }

        if (maximumLength <= Ellipsis.Length)
        {
            return new string('.', maximumLength);
        }

        var contentLength = maximumLength - Ellipsis.Length;
        return string.Concat(
            escaped.AsSpan(0, contentLength),
            Ellipsis.AsSpan());
    }
}
