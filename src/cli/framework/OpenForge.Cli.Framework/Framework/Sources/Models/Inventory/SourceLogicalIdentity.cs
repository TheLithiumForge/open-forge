using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

internal sealed class SourceLogicalIdentity
{
    internal SourceLogicalIdentity(
        string automaticId,
        string canonicalBasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(automaticId);
        if (!SourceWorkspaceRelativePath.IsCanonical(canonicalBasePath)
            || canonicalBasePath == SourceLogicalPath.AgentsRoot
            || SourceLogicalPath.IsCanonicalSource(canonicalBasePath) && SourceOverwritePath.HasSuffix(canonicalBasePath))
        {
            throw new ArgumentException("The logical source base path is not canonical.", nameof(canonicalBasePath));
        }

        if (!SourceLogicalPath.IsCanonicalSource(canonicalBasePath) && automaticId != $"./{canonicalBasePath}")
        {
            throw new ArgumentException("An included source requires its exact workspace reference.", nameof(automaticId));
        }

        AutomaticId = automaticId;
        CanonicalBasePath = canonicalBasePath;
    }

    internal string AutomaticId { get; }

    internal string CanonicalBasePath { get; }
}
