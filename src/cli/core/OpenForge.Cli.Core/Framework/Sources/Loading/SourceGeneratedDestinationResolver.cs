using System.Text;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Framework.Sources.Loading;

internal static class SourceGeneratedDestinationResolver
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static string? Resolve(
        string parentCanonicalPath,
        bool isLoader,
        string destination)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parentCanonicalPath);
        ArgumentNullException.ThrowIfNull(destination);
        try
        {
            var decoded = Decode(destination);
            if (decoded is null || decoded.Length == 0 || decoded.StartsWith("/", StringComparison.Ordinal)
                || decoded.Contains('\\') || decoded.Contains('?') || decoded.Contains('#') || decoded.Contains(':'))
            {
                return null;
            }

            var basePath = isLoader ? SourceLogicalPath.AgentsRoot : SourceLogicalPath.ReadParent(parentCanonicalPath);
            var segments = basePath.Split('/').ToList();
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

                if (!SourceLogicalPath.IsCanonicalSegment(segment))
                {
                    return null;
                }

                segments.Add(segment);
            }

            var path = string.Join('/', segments);
            return SourceLogicalPath.IsCanonical(path) ? path : null;
        }
        catch (DecoderFallbackException)
        {
            return null;
        }
    }

    private static string? Decode(string destination)
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

    private static bool TryHex(char value, out int result)
    {
        result = value switch
        {
            >= '0' and <= '9' => value - '0',
            >= 'a' and <= 'f' => value - 'a' + 10,
            >= 'A' and <= 'F' => value - 'A' + 10,
            _ => -1,
        };
        return result >= 0;
    }
}
