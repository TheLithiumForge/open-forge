using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Shared.Destinations;

namespace OpenForge.Cli.Core.Framework.Sources.Loading;

internal static class SourceGeneratedDestinationResolver
{
    internal static string? Resolve(
        string parentCanonicalPath,
        bool isLoader,
        string destination)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parentCanonicalPath);
        ArgumentNullException.ThrowIfNull(destination);
        if (!SourceDestinationDecoder.TryDecode(destination, out var decoded, out _)
            || decoded.Length == 0 || decoded.StartsWith("/", StringComparison.Ordinal)
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
}
