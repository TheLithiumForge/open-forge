using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

internal sealed class SourceLogicalIdentity
{
    internal SourceLogicalIdentity(
        string automaticId,
        string canonicalBasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(automaticId);
        if (!SourceLogicalPath.IsCanonicalSource(canonicalBasePath)
            || SourceOverwritePath.HasSuffix(canonicalBasePath))
        {
            throw new ArgumentException("The logical source base path is not canonical.", nameof(canonicalBasePath));
        }

        AutomaticId = automaticId;
        CanonicalBasePath = canonicalBasePath;
    }

    internal string AutomaticId { get; }

    internal string CanonicalBasePath { get; }
}
