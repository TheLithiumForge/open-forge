using System.Buffers;
using System.Text;

namespace OpenForge.Cli.Core.Framework.Extensions.Identity;

internal static class ExtensionTargetPath
{
    private static readonly SearchValues<char> ForbiddenCharacters = SearchValues.Create(
        "<>:\"|?*\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\u0008\u0009\u000A\u000B\u000C\u000D\u000E\u000F\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001A\u001B\u001C\u001D\u001E\u001F");

    internal static bool TryNormalize(string? value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value)
            || Path.IsPathRooted(value)
            || value.Contains('\\', StringComparison.Ordinal)
            || value[^1] == '/'
            || !value.IsNormalized(NormalizationForm.FormC))
        {
            return false;
        }

        var segments = value.Split('/');
        if (segments.Any(IsInvalidSegment))
        {
            return false;
        }

        normalized = string.Join('/', segments);
        return normalized.Length <= 4096;
    }

    internal static string CreatePortableKey(string normalizedPath)
        => normalizedPath.ToLowerInvariant();

    private static bool IsInvalidSegment(string segment)
    {
        if (segment.Length == 0
            || segment is "." or ".."
            || segment[^1] is '.' or ' '
            || segment.AsSpan().ContainsAny(ForbiddenCharacters))
        {
            return true;
        }

        var dotIndex = segment.IndexOf('.');
        var stem = segment.AsSpan();
        if (dotIndex >= 0)
        {
            stem = stem[..dotIndex];
        }

        return IsReservedDeviceName(stem);
    }

    private static bool IsReservedDeviceName(ReadOnlySpan<char> stem)
    {
        if (stem.Equals("CON", StringComparison.OrdinalIgnoreCase)
            || stem.Equals("PRN", StringComparison.OrdinalIgnoreCase)
            || stem.Equals("AUX", StringComparison.OrdinalIgnoreCase)
            || stem.Equals("NUL", StringComparison.OrdinalIgnoreCase)
            || stem.Equals("CLOCK$", StringComparison.OrdinalIgnoreCase)
            || stem.Equals("CONIN$", StringComparison.OrdinalIgnoreCase)
            || stem.Equals("CONOUT$", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return stem.Length == 4
            && (stem[..3].Equals("COM", StringComparison.OrdinalIgnoreCase)
                || stem[..3].Equals("LPT", StringComparison.OrdinalIgnoreCase))
            && stem[3] is >= '1' and <= '9' or '\u00B9' or '\u00B2' or '\u00B3';
    }
}
