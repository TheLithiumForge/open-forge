using OpenForge.Cli.Core.Framework.Sources.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Shared.Destinations;

namespace OpenForge.Cli.Core.Framework.Sources.Routing;

internal static class SourceLoaderDestinationParser
{
    internal static SourceLoaderDestinationParseResult Parse(string attemptedDestination)
    {
        ArgumentNullException.ThrowIfNull(attemptedDestination);
        if (attemptedDestination.Length == 0)
        {
            return SourceLoaderDestinationParseResult.Malformed(
                attemptedDestination: attemptedDestination,
                cause: "A Loader destination cannot be empty.");
        }

        if (!SourceDestinationDecoder.TryDecode(attemptedDestination, out var decodedDestination, out var failure))
        {
            var cause = failure switch
            {
                SourceDestinationDecodeFailure.UnencodedWhitespace => "Unencoded whitespace is not valid in a Loader destination.",
                SourceDestinationDecodeFailure.InvalidPercentTriplet => "Every percent sign must begin a valid percent triplet.",
                SourceDestinationDecodeFailure.InvalidUtf8 => "Percent-encoded bytes must form strict UTF-8.",
                SourceDestinationDecodeFailure.None => throw new InvalidOperationException("Failed destination decoding requires a failure cause."),
                _ => throw new ArgumentOutOfRangeException(nameof(failure), failure, "The destination decoding failure is not defined."),
            };
            return SourceLoaderDestinationParseResult.Malformed(attemptedDestination: attemptedDestination, cause: cause);
        }

        if (ContainsUnsafeDestinationCharacter(decodedDestination))
        {
            return SourceLoaderDestinationParseResult.Unsafe(
                attemptedDestination: attemptedDestination,
                decodedDestination: decodedDestination,
                cause: "The decoded Loader destination contains an unsafe path character.");
        }

        var segments = decodedDestination.Split('/', StringSplitOptions.None);
        if (segments.Any(segment => segment.Length == 0 || segment is "." or ".."))
        {
            return SourceLoaderDestinationParseResult.Unsafe(
                attemptedDestination: attemptedDestination,
                decodedDestination: decodedDestination,
                cause: "The decoded Loader destination contains an empty or traversal segment.");
        }

        return SourceLoaderDestinationParseResult.Valid(
            attemptedDestination: attemptedDestination,
            decodedDestination: decodedDestination,
            canonicalPath: $".agents/{decodedDestination}");
    }

    private static bool ContainsUnsafeDestinationCharacter(string value)
    {
        if (value.Length == 0 || value[0] == '/' || value.Contains('\\'))
        {
            return true;
        }

        return value.Any(character => char.IsControl(character) || character is '?' or '#' or ':');
    }
}
