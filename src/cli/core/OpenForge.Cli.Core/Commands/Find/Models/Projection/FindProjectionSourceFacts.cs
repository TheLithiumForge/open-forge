using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Find.Models.Projection;

internal sealed record FindProjectionSourceInput(
    FindMatch Match,
    IEnumerable<FindLayerInspectionFacts> Inspections,
    SourceRouteFacts? RouteFacts);

internal sealed record FindProjectionSourceFacts(
    FindMatch Match,
    FindSourceIdentity SourceIdentity,
    SourceLogicalSource? Source,
    IReadOnlyList<FindProjectionLayer> Layers,
    FindSourceRouteProjection? Route);

internal sealed record FindProjectionLayer(
    SourceLayerKind Kind,
    string Path,
    FindLayerInspectionFacts? Inspection);

internal sealed record FindSourceRouteProjection(
    FindRouteState State,
    string? Route);
