using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

namespace OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;

internal sealed record GeneratedNavigationMetadata
{
    internal GeneratedNavigationMetadata(
        SourceLogicalSource source,
        SourceAuthoredMetadataFacts facts)
    {
        Source = source;
        Facts = facts;
    }

    internal SourceLogicalSource Source { get; }

    internal SourceAuthoredMetadataFacts Facts { get; }
}
