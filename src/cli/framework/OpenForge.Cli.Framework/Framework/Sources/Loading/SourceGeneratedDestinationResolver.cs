using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Shared.Destinations;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Sources.Loading;

internal static class SourceGeneratedDestinationResolver
{

    internal static string? Resolve(WorkspaceLayout layout, string parentCanonicalPath, bool isLoader, string destination)
    {
        var roots = new SourceContentRoots(layout);
        if (!isLoader && SourceLogicalPath.IsOutsideSource(layout, parentCanonicalPath))
        {
            var boundary = layout.OutsideRoutes.Single(route => parentCanonicalPath == route.DirectoryPath
                || parentCanonicalPath.StartsWith(route.DirectoryPath + "/", StringComparison.Ordinal));
            var path = ResolveWorkspacePath(roots, parentCanonicalPath, isLoader: false, destination, minimumDepth: boundary.DirectoryPath.Split('/').Length);
            return path is not null && path.StartsWith(boundary.DirectoryPath + "/", StringComparison.Ordinal) ? path : null;
        }
        return Resolve(roots, parentCanonicalPath, isLoader, destination);
    }

    internal static string? Resolve(SourceContentRoots roots, string parentCanonicalPath, bool isLoader, string destination)
    {
        var path = ResolveWorkspacePath(roots, parentCanonicalPath, isLoader, destination, minimumDepth: roots.IsDefault ? 1 : 0);
        return path is not null && SourceLogicalPath.IsPrimarySource(roots, path) ? path : null;
    }

    internal static bool IsAuthoredLink(string destination)
        => TryDecode(destination, out var decoded) && decoded.Split('/').Contains("..");

    internal static bool IsMarkdown(string path) => path.EndsWith(SourceFormClassifier.MarkdownExtension, StringComparison.OrdinalIgnoreCase);

    internal static string? ResolveLinked(WorkspaceLayout layout, string parentPath, bool isLoader, string destination)
        => ResolveLinked(new SourceContentRoots(layout), parentPath, isLoader, destination);

    internal static string? ResolveLinked(SourceContentRoots roots, string parentPath, bool isLoader, string destination)
    {
        var path = ResolveWorkspacePath(roots, parentPath, isLoader, destination, minimumDepth: 0);
        return path is not null && !SourceLogicalPath.IsPrimarySource(roots, path) && !roots.SourceRoots.Contains(path) ? path : null;
    }

    private static string? ResolveWorkspacePath(SourceContentRoots roots, string parentPath, bool isLoader, string destination, int minimumDepth)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parentPath);
        ArgumentNullException.ThrowIfNull(destination);
        if (!TryDecode(destination, out var decoded)) return null;
        var segments = ReadFolder(isLoader ? roots.LoaderPath : parentPath)
            .Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
        foreach (var segment in decoded.Split('/', StringSplitOptions.None))
        {
            if (segment.Length == 0 || segment == ".") continue;
            if (segment == "..")
            {
                if (segments.Count <= minimumDepth) return null;
                segments.RemoveAt(segments.Count - 1);
                continue;
            }
            if (!SourceLogicalPath.IsCanonicalSegment(segment)) return null;
            segments.Add(segment);
        }
        var path = string.Join('/', segments);
        return SourceWorkspaceRelativePath.IsCanonical(path) ? path : null;
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
