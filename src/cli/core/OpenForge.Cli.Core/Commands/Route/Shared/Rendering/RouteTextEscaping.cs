using System.Text;
using System.Text.Json;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Rendering;

internal static class RouteTextEscaping
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
        var builder = new StringBuilder(contentLength);
        foreach (var scalar in value.EnumerateRunes())
        {
            var encodedScalar = Escape(scalar.ToString());
            if (builder.Length + encodedScalar.Length > contentLength)
            {
                break;
            }

            builder.Append(encodedScalar);
        }

        builder.Append(Ellipsis);
        return builder.ToString();
    }
}
