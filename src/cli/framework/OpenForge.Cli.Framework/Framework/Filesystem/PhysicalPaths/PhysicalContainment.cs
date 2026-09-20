namespace OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

internal static class PhysicalContainment
{
    internal static bool Contains(string rootPath, string candidatePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidatePath);
        var root = Path.GetFullPath(rootPath);
        var candidate = Path.GetFullPath(candidatePath);
        var relative = Path.GetRelativePath(root, candidate);
        if (relative == ".")
        {
            return true;
        }

        if (Path.IsPathRooted(relative) || relative == "..")
        {
            return false;
        }

        return !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            && !relative.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal);
    }
}
