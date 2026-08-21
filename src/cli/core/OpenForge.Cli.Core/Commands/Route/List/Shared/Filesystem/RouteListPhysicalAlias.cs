namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal sealed record RouteListPhysicalAlias
{
    internal RouteListPhysicalAlias(
        string canonicalLogicalPath,
        string physicalPath,
        string firstCanonicalLogicalPath)
    {
        if (!RouteListLogicalPath.IsCanonical(canonicalLogicalPath))
        {
            throw new ArgumentException("The alias logical path is not canonical.", nameof(canonicalLogicalPath));
        }

        if (!RouteListLogicalPath.IsCanonical(firstCanonicalLogicalPath))
        {
            throw new ArgumentException("The first alias logical path is not canonical.", nameof(firstCanonicalLogicalPath));
        }

        if (string.Equals(canonicalLogicalPath, firstCanonicalLogicalPath, StringComparison.Ordinal))
        {
            throw new ArgumentException("A physical alias must have a different first logical path.", nameof(firstCanonicalLogicalPath));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(physicalPath);
        var normalizedPhysicalPath = Path.GetFullPath(physicalPath);
        if (!Path.IsPathRooted(physicalPath)
            || !string.Equals(normalizedPhysicalPath, physicalPath, StringComparison.Ordinal))
        {
            throw new ArgumentException("The alias physical path must be absolute and normalized.", nameof(physicalPath));
        }

        CanonicalLogicalPath = canonicalLogicalPath;
        PhysicalPath = normalizedPhysicalPath;
        FirstCanonicalLogicalPath = firstCanonicalLogicalPath;
    }

    internal string CanonicalLogicalPath { get; }

    internal string PhysicalPath { get; }

    internal string FirstCanonicalLogicalPath { get; }
}
