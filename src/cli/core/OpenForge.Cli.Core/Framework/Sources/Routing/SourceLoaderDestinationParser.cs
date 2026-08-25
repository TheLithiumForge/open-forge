using System.Diagnostics.CodeAnalysis;
using System.Text;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Framework.Sources.Routing;

internal static class SourceLoaderDestinationParser
{
    internal static SourceLoaderDestinationParseResult Parse(string attemptedDestination)
    {
        ArgumentNullException.ThrowIfNull(attemptedDestination);
        if (attemptedDestination.Length == 0)
        {
            return SourceLoaderDestinationParseResult.Malformed(
                attemptedDestination,
                "A Loader destination cannot be empty.");
        }

        if (!TryDecode(attemptedDestination, out var decodedDestination, out var decodeCause))
        {
            return SourceLoaderDestinationParseResult.Malformed(
                attemptedDestination,
                decodeCause);
        }

        if (ContainsUnsafeDestinationCharacter(decodedDestination))
        {
            return SourceLoaderDestinationParseResult.Unsafe(
                attemptedDestination,
                decodedDestination,
                "The decoded Loader destination contains an unsafe path character.");
        }

        var segments = decodedDestination.Split('/', StringSplitOptions.None);
        if (segments.Any(segment => segment.Length == 0 || segment is "." or ".."))
        {
            return SourceLoaderDestinationParseResult.Unsafe(
                attemptedDestination,
                decodedDestination,
                "The decoded Loader destination contains an empty or traversal segment.");
        }

        return SourceLoaderDestinationParseResult.Valid(
            attemptedDestination,
            decodedDestination,
            ".agents/" + decodedDestination);
    }

    private static bool TryDecode(
        string attemptedDestination,
        [NotNullWhen(true)] out string? decodedDestination,
        [NotNullWhen(false)] out string? cause)
    {
        decodedDestination = null;
        var builder = new StringBuilder(attemptedDestination.Length);
        for (var index = 0; index < attemptedDestination.Length;)
        {
            var character = attemptedDestination[index];
            if (char.IsWhiteSpace(character))
            {
                cause = "Unencoded whitespace is not valid in a Loader destination.";
                return false;
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
                    return false;
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
                return false;
            }
        }

        decodedDestination = builder.ToString();
        cause = null;
        return true;
    }

    private static bool ContainsUnsafeDestinationCharacter(string value)
    {
        if (value.Length == 0 || value[0] == '/' || value.Contains('\\'))
        {
            return true;
        }

        return value.Any(character => char.IsControl(character) || character is '?' or '#' or ':');
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
