using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Shared.Destinations;

namespace OpenForge.Cli.Core.Framework.Sources.Loading;

internal static class SourceGeneratedDestinationResolver
{
    private const string MarkdownExtension = ".md";

    internal static string? Resolve(
        string parentCanonicalPath,
        bool isLoader,
        string destination)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parentCanonicalPath);
        ArgumentNullException.ThrowIfNull(destination);
        if (!TryDecode(destination, out var decoded))
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

    /// <summary>
    /// True when an Entries destination climbs with `..`. Generated rows always name direct children, so
    /// such a row was written by a person, usually to link a document outside `.agents/`. Maintenance keeps
    /// these rows as written and never treats them as stale generated navigation.
    /// </summary>
    internal static bool IsAuthoredLink(string destination)
        => TryDecode(destination, out var decoded) && decoded.Split('/').Contains("..");

    /// <summary>
    /// Resolves an Entries destination that leaves `.agents/` but stays inside the workspace, such as
    /// `../docs/architecture.md`. Returns the workspace-relative Markdown path, or null when the
    /// destination stays inside `.agents/`, escapes the workspace, or is not a Markdown file.
    /// </summary>
    internal static string? ResolveLinked(
        string parentPath,
        bool isLoader,
        string destination)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parentPath);
        ArgumentNullException.ThrowIfNull(destination);
        if (!TryDecode(destination, out var decoded))
        {
            return null;
        }

        var segments = (isLoader ? SourceLogicalPath.AgentsRoot : ReadFolder(parentPath))
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .ToList();
        foreach (var segment in decoded.Split('/', StringSplitOptions.None))
        {
            if (segment.Length == 0 || segment == ".")
            {
                continue;
            }

            if (segment == "..")
            {
                if (segments.Count == 0)
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

        if (segments.Count == 0
            || segments[0] == SourceLogicalPath.AgentsRoot
            || !segments[^1].EndsWith(MarkdownExtension, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return string.Join('/', segments);
    }

    private static string ReadFolder(string path)
    {
        var separator = path.LastIndexOf('/');
        return separator < 0 ? string.Empty : path[..separator];
    }

    private static bool TryDecode(string destination, [NotNullWhen(true)] out string? decoded)
    {
        decoded = null;
        return !destination.Contains('?')
            && !destination.Contains('#')
            && !destination.Contains(':')
            && SourceDestinationDecoder.TryDecode(destination, out decoded, out _)
            && decoded.Length > 0
            && !decoded.StartsWith("/", StringComparison.Ordinal)
            && !decoded.Contains('\\');
    }
}
