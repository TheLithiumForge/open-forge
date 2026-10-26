using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.List.Models;

internal sealed record RouteListSourceResolution(
    WorkspaceLayoutRead LayoutRead,
    RouteListInventoryFacts Inventory,
    SourceRouteFacts RouteFacts,
    RouteListSelectionResolution Selection)
{
    internal IReadOnlyList<SourceIncludedEntryRow> IncludedRows { get; init; } = [];

    internal IReadOnlyList<SourceLogicalSource> IncludedSources { get; init; } = [];

    internal IReadOnlyList<string> UnlistedRoots { get; init; } = [];
}
