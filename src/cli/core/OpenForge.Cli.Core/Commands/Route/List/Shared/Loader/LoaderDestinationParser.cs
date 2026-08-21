using System.Text;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Loader;

internal enum LoaderDestinationParseState
{
    Valid,
    Malformed,
    Unsafe,
}

internal sealed class LoaderDestinationParseResult
{
    private LoaderDestinationParseResult(
        LoaderDestinationParseState state,
        string attemptedDestination,
        string? decodedDestination,
        string? canonicalPath,
        string? cause)
    {
        State = state;
        AttemptedDestination = attemptedDestination;
        DecodedDestination = decodedDestination;
        CanonicalPath = canonicalPath;
        Cause = cause;
    }

    internal LoaderDestinationParseState State { get; }

    internal string AttemptedDestination { get; }

    internal string? DecodedDestination { get; }

    internal string? CanonicalPath { get; }

    internal string? Cause { get; }

    internal static LoaderDestinationParseResult Valid(
        string attemptedDestination,
        string decodedDestination,
        string canonicalPath)
    {
        return new LoaderDestinationParseResult(
            LoaderDestinationParseState.Valid,
            attemptedDestination,
            decodedDestination,
            canonicalPath,
            null);
    }

    internal static LoaderDestinationParseResult Malformed(string attemptedDestination, string cause)
    {
        return new LoaderDestinationParseResult(
            LoaderDestinationParseState.Malformed,
            attemptedDestination,
            null,
            null,
            cause);
    }

    internal static LoaderDestinationParseResult Unsafe(
        string attemptedDestination,
        string decodedDestination,
        string cause)
    {
        return new LoaderDestinationParseResult(
            LoaderDestinationParseState.Unsafe,
            attemptedDestination,
            decodedDestination,
            null,
            cause);
    }
}

internal static class LoaderDestinationParser
{
    internal static LoaderDestinationParseResult Parse(string attemptedDestination)
    {
        ArgumentNullException.ThrowIfNull(attemptedDestination);
        if (attemptedDestination.Length == 0)
        {
            return LoaderDestinationParseResult.Malformed(
                attemptedDestination,
                "A Loader destination cannot be empty.");
        }

        var decoded = Decode(attemptedDestination, out var decodeCause, out var malformed);
        if (malformed)
        {
            return LoaderDestinationParseResult.Malformed(attemptedDestination, decodeCause!);
        }

        if (ContainsUnsafeDestinationCharacter(decoded!))
        {
            return LoaderDestinationParseResult.Unsafe(
                attemptedDestination,
                decoded!,
                "The decoded Loader destination contains an unsafe path character.");
        }

        var segments = decoded!.Split('/', StringSplitOptions.None);
        if (segments.Any(segment => segment.Length == 0 || segment is "." or ".."))
        {
            return LoaderDestinationParseResult.Unsafe(
                attemptedDestination,
                decoded,
                "The decoded Loader destination contains an empty or traversal segment.");
        }

        return LoaderDestinationParseResult.Valid(
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
