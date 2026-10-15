using OpenForge.Cli.Core.Commands.Repair.Models.Selection;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Planning;

internal static class RepairDestinationFormatter
{
    internal static string Format(string sourceCanonicalPath, RepairTargetSelection target)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCanonicalPath);
        ArgumentNullException.ThrowIfNull(target);
        return AppendFragment(FormatPath(sourceCanonicalPath, target.CanonicalTargetPath), target.TargetFragment);
    }

    internal static string FormatPath(string sourceCanonicalPath, string canonicalTargetPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCanonicalPath);
        var sourceDirectory = Path.GetDirectoryName(
                sourceCanonicalPath.Replace('/', Path.DirectorySeparatorChar))
            ?? string.Empty;
        var path = Path.GetRelativePath(
                sourceDirectory.Length == 0 ? "." : sourceDirectory,
                Uri.UnescapeDataString(canonicalTargetPath).Replace('/', Path.DirectorySeparatorChar))
            .Replace(Path.DirectorySeparatorChar, '/');
        path = string.Join("/", path.Split('/').Select(Uri.EscapeDataString));
        return path;
    }

    internal static string AppendFragment(string path, string? fragment)
        => fragment is null
            ? path
            : $"{path}#{Uri.EscapeDataString(Uri.UnescapeDataString(fragment))}";
}
