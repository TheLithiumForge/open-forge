using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;

internal static class ToolNativePaths
{
    internal static StringComparer Comparer => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    internal static string Normalize(string path) => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

    internal static CliWorkspace Scope(ToolRecipeContext context, string path)
    {
        if (!Path.IsPathFullyQualified(path)) throw new InvalidDataException("A native output must be an absolute path.");
        path = Normalize(path);
        var roots = new[] { context.WorkspacePath, context.HomePath, context.NativeHomePath, context.BinPath, context.NativeInstallDirectory }.Concat(context.ClientRoots.Values);
        var root = roots.OrderByDescending(value => value.Length).FirstOrDefault(value => PhysicalContainment.Contains(value, path))
            ?? throw new InvalidDataException("A native path is outside the declared roots.");
        root = Normalize(root);
        while (!Directory.Exists(root)) root = Path.GetDirectoryName(root) ?? throw new IOException("The native path has no existing ancestor.");
        var scope = ToolFileReader.Scope(root);
        if (Comparer.Equals(root, path)) throw new InvalidDataException("A native file target cannot be a declared root directory.");
        _ = ToolFileReader.Observe(scope, path, CancellationToken.None);
        return scope;
    }

    internal static string HomeIdentity(string path)
    {
        var normalized = Normalize(path);
        var physical = new PhysicalPathResolver().ResolveRoot(normalized);
        if (physical.State == PhysicalPathState.Missing) return normalized;
        if (physical.State != PhysicalPathState.Contained) throw new IOException("The native home is unavailable.");
        return Normalize(physical.GetContainedPhysicalPath());
    }
}
