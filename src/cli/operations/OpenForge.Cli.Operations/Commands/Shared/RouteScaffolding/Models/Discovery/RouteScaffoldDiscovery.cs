using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Sharing;

namespace OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Discovery;

internal sealed record RouteScaffoldDiscoveryRequest
{
    public required SourceCatalogue Catalogue { get; init; }
    // These are caller-selected roots, including explicitly selected outside anchors.
    public required ImmutableArray<string> RecognizedRootPaths { get; init; }
    public required ImmutableArray<SourceLogicalSource> EligibleSources { get; init; }
    public required WorkspaceSettingsDocument Settings { get; init; }
    public SourceSharing? Sharing { get; init; }
    public ImmutableArray<string> OccupiedPaths { get; init; } = [];
}

internal enum RouteScaffoldBoundaryKind
{
    EntrypointAmbiguous,
    IdentityOccupied,
    PathOccupied,
    SharedEntrypointMissing,
}

internal sealed record RouteScaffoldBoundary(RouteScaffoldBoundaryKind Kind, string Path);

internal sealed record RouteScaffoldGap(string FolderId, string CanonicalPath);

internal sealed record RouteScaffoldDiscovery(
    ImmutableArray<RouteScaffoldGap> Gaps,
    ImmutableArray<RouteScaffoldBoundary> Boundaries)
{
    internal bool IsBlocked => !Boundaries.IsEmpty;
}
