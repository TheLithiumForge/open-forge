using System.Text;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal sealed partial class RouteInspectLoadingFactsBuilder
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private static bool HasTag(RouteSource source, string tag)
    {
        return source.Metadata.Tags.Contains(tag, StringComparer.Ordinal);
    }

    private static string? ResolveDestination(string parentPath, string destination)
    {
        try
        {
            var decoded = DecodeDestination(destination);
            if (decoded is null || decoded.Length == 0 || decoded.StartsWith("/", StringComparison.Ordinal)
                || decoded.Contains('\\') || decoded.Contains('?') || decoded.Contains('#') || decoded.Contains(':'))
            {
                return null;
            }

            var segments = parentPath.Split('/').ToList();
            foreach (var segment in decoded.Split('/', StringSplitOptions.None))
            {
                if (segment.Length == 0 || segment == ".")
                {
                    continue;
                }

                if (segment == "..")
                {
                    if (segments.Count <= 1)
                    {
                        return null;
                    }

                    segments.RemoveAt(segments.Count - 1);
                    continue;
                }

                if (!RouteLogicalPath.IsCanonicalSegment(segment))
                {
                    return null;
                }

                segments.Add(segment);
            }

            var path = string.Join('/', segments);
            return RouteLogicalPath.IsCanonical(path) ? path : null;
        }
        catch (DecoderFallbackException)
        {
            return null;
        }
    }

    private static string? DecodeDestination(string destination)
    {
        var builder = new StringBuilder(destination.Length);
        for (var index = 0; index < destination.Length;)
        {
            var character = destination[index];
            if (char.IsWhiteSpace(character))
            {
                return null;
            }

            if (character != '%')
            {
                builder.Append(character);
                index++;
                continue;
            }

            var bytes = new List<byte>();
            while (index < destination.Length && destination[index] == '%')
            {
                if (index + 2 >= destination.Length
                    || !TryHex(destination[index + 1], out var high)
                    || !TryHex(destination[index + 2], out var low))
                {
                    return null;
                }

                bytes.Add((byte)((high << 4) | low));
                index += 3;
            }

            builder.Append(StrictUtf8.GetString(bytes.ToArray()));
        }

        return builder.ToString();
    }

    private static bool TryHex(char character, out int value)
    {
        if (character is >= '0' and <= '9')
        {
            value = character - '0';
            return true;
        }

        if (character is >= 'A' and <= 'F')
        {
            value = character - 'A' + 10;
            return true;
        }

        if (character is >= 'a' and <= 'f')
        {
            value = character - 'a' + 10;
            return true;
        }

        value = 0;
        return false;
    }

    private void CheckCancellation()
    {
        _cancellationToken.ThrowIfCancellationRequested();
    }
}
