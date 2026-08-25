using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

internal sealed class SourceCatalogueSelectionScope
{
    internal SourceCatalogueSelectionScope(
        string canonicalDirectoryPath,
        string physicalDirectoryPath)
    {
        if (!SourceLogicalPath.IsCanonicalRoot(canonicalDirectoryPath))
        {
            throw new ArgumentException("The selection scope directory path is not canonical.", nameof(canonicalDirectoryPath));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(physicalDirectoryPath);
        var normalizedPhysicalDirectoryPath = Path.GetFullPath(physicalDirectoryPath);
        if (!Path.IsPathRooted(physicalDirectoryPath)
            || !string.Equals(normalizedPhysicalDirectoryPath, physicalDirectoryPath, StringComparison.Ordinal))
        {
            throw new ArgumentException("The selection scope physical directory must be absolute and normalized.", nameof(physicalDirectoryPath));
        }

        CanonicalDirectoryPath = canonicalDirectoryPath;
        PhysicalDirectoryPath = normalizedPhysicalDirectoryPath;
    }

    internal string CanonicalDirectoryPath { get; }

    internal string PhysicalDirectoryPath { get; }
}
