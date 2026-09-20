namespace OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

internal sealed class PhysicalIdentityTracker
{
    private readonly Dictionary<string, string> _activeLogicalByPhysical = new(PathComparer);

    internal static StringComparer PathComparer => OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    internal string? Enter(string logicalPath, string resolvedPhysicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(resolvedPhysicalPath);
        var normalized = Path.GetFullPath(resolvedPhysicalPath);
        if (_activeLogicalByPhysical.TryGetValue(normalized, out var existing))
        {
            return existing;
        }

        _activeLogicalByPhysical.Add(normalized, logicalPath);
        return null;
    }

    internal void Exit(string resolvedPhysicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resolvedPhysicalPath);
        var normalized = Path.GetFullPath(resolvedPhysicalPath);
        if (!_activeLogicalByPhysical.Remove(normalized))
        {
            throw new InvalidOperationException("The physical identity was not active.");
        }
    }
}
