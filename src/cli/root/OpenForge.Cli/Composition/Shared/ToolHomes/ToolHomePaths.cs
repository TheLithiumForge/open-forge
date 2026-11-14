using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;

namespace OpenForge.Cli.Composition.Shared.ToolHomes;

internal static class ToolHomePaths
{
    internal static string Join(string root, string suffix) => Path.Join(root, suffix.Replace('/', Path.DirectorySeparatorChar));

    internal static string Resolve(string path, bool directory)
    {
        if (string.IsNullOrWhiteSpace(path) || !Path.IsPathFullyQualified(path))
            throw new IOException("The native path must be absolute.");
        var current = Path.GetFullPath(path);
        var missing = new Stack<string>();
        while (LinkTargetReader.Read(current).State == PathComponentState.Missing)
        {
            missing.Push(Path.GetFileName(current));
            current = Path.GetDirectoryName(current) ?? throw new IOException("The native path has no existing ancestor.");
        }
        var resolved = new PhysicalPathResolver().ResolveRoot(current);
        if (resolved.State != PhysicalPathState.Contained)
            throw new IOException($"The native path cannot be resolved safely: {path}.");
        current = resolved.GetContainedPhysicalPath();
        if ((directory || missing.Count > 0) && !Directory.Exists(current))
            throw new IOException($"The native directory path contains a file: {path}.");
        if (!directory && missing.Count == 0 && Directory.Exists(current))
            throw new IOException($"The native file path names a directory: {path}.");
        foreach (var segment in missing) current = Path.Combine(current, segment);
        return Path.TrimEndingDirectorySeparator(current);
    }

    internal static bool Exists(string path)
    {
        var resolved = Resolve(path, directory: false);
        return File.Exists(resolved);
    }

    internal static string Choose(string root, params string[] suffixes)
    {
        foreach (var suffix in suffixes)
        {
            var path = Join(root, suffix);
            if (Exists(path)) return path;
        }
        return Join(root, suffixes[0]);
    }
}
