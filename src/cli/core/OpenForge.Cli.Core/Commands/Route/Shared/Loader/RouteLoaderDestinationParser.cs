using OpenForge.Cli.Core.Commands.Route.Shared.Models.Loader;
using System.Text;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Loader;

internal static class RouteLoaderDestinationParser
{
    internal static RouteLoaderDestinationParseResult Parse(string attemptedDestination)
    {
        ArgumentNullException.ThrowIfNull(attemptedDestination);
        if (attemptedDestination.Length == 0)
        {
            return RouteLoaderDestinationParseResult.Malformed(
                attemptedDestination,
                "A Loader destination cannot be empty.");
        }

        var decoded = Decode(attemptedDestination, out var decodeCause, out var malformed);
        if (malformed)
        {
            return RouteLoaderDestinationParseResult.Malformed(attemptedDestination, decodeCause!);
        }

        if (ContainsUnsafeDestinationCharacter(decoded!))
        {
            return RouteLoaderDestinationParseResult.Unsafe(
                attemptedDestination,
                decoded!,
                "The decoded Loader destination contains an unsafe path character.");
        }

        var segments = decoded!.Split('/', StringSplitOptions.None);
        if (segments.Any(segment => segment.Length == 0 || segment is "." or ".."))
        {
            return RouteLoaderDestinationParseResult.Unsafe(
                attemptedDestination,
                decoded,
                "The decoded Loader destination contains an empty or traversal segment.");
        }

        return RouteLoaderDestinationParseResult.Valid(
            attemptedDestination,
            decoded,
            ".agents/" + decoded);
    }

    private static string? Decode(
        string attemptedDestination,
        out string? cause,
        out bool malformed)
    {
        var builder = new StringBuilder(attemptedDestination.Length);
        for (var index = 0; index < attemptedDestination.Length;)
        {
            var character = attemptedDestination[index];
            if (char.IsWhiteSpace(character))
            {
                cause = "Unencoded whitespace is not valid in a Loader destination.";
                malformed = true;
                return null;
            }

            if (character != '%')
            {
                builder.Append(character);
                index++;
                continue;
            }

            var bytes = new List<byte>();
            while (index < attemptedDestination.Length && attemptedDestination[index] == '%')
            {
                if (index + 2 >= attemptedDestination.Length
                    || !TryHex(attemptedDestination[index + 1], out var high)
                    || !TryHex(attemptedDestination[index + 2], out var low))
                {
                    cause = "Every percent sign must begin a valid percent triplet.";
                    malformed = true;
                    return null;
                }

                bytes.Add((byte)((high << 4) | low));
                index += 3;
            }

            try
            {
                builder.Append(StrictUtf8.GetString(bytes.ToArray()));
            }
            catch (DecoderFallbackException)
            {
                cause = "Percent-encoded bytes must form strict UTF-8.";
                malformed = true;
                return null;
            }
        }

        cause = null;
        malformed = false;
        return builder.ToString();
    }

    private static bool ContainsUnsafeDestinationCharacter(string value)
    {
        if (value.Length == 0 || value[0] == '/' || value.Contains('\\'))
        {
            return true;
        }

        foreach (var character in value)
        {
            if (char.IsControl(character) || character is '?' or '#' or ':')
            {
                return true;
            }
        }

        return false;
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

    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
}
