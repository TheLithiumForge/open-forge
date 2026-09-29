using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;

internal static class SourceWorkingPathNormalizer
{
    internal static SourceWorkingPathResult Normalize(
        string workspaceRoot,
        IReadOnlyList<string> paths)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRoot);
        ArgumentNullException.ThrowIfNull(paths);

        var root = Path.GetFullPath(workspaceRoot);
        var normalizedPaths = new List<string>(paths.Count);
        var invalidPaths = new List<string>();
        var seenPaths = new HashSet<string>(StringComparer.Ordinal);

        foreach (var path in paths)
        {
            if (!TryNormalize(root, path, out var normalizedPath))
            {
                invalidPaths.Add(path ?? string.Empty);
                continue;
            }

            if (seenPaths.Add(normalizedPath))
            {
                normalizedPaths.Add(normalizedPath);
            }
        }

        return new SourceWorkingPathResult(normalizedPaths, invalidPaths);
    }

    private static bool TryNormalize(string workspaceRoot, string? path, out string normalizedPath)
    {
        normalizedPath = string.Empty;
        if (string.IsNullOrWhiteSpace(path) || path.Any(char.IsControl))
        {
            return false;
        }

        try
        {
            var fullPath = Path.GetFullPath(path, workspaceRoot);
            var relativePath = Path.GetRelativePath(workspaceRoot, fullPath);
            if (relativePath == "."
                || Path.IsPathRooted(relativePath)
                || relativePath == ".."
                || relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                || relativePath.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal))
            {
                return false;
            }

            normalizedPath = relativePath
                .Replace(Path.DirectorySeparatorChar, '/')
                .Replace(Path.AltDirectorySeparatorChar, '/');
            return normalizedPath.Length > 0;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
        catch (PathTooLongException)
        {
            return false;
        }
    }
}
