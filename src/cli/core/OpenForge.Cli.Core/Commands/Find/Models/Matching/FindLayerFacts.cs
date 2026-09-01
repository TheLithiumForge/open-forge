using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Find.Models.Matching;

internal sealed record FindLayerFactsInput(
    SourceLogicalSource Source,
    IEnumerable<FindLayerInspectionFacts> Inspections);

internal sealed record FindLayerFacts(
    SourceLogicalSource Source,
    SourceLayer Layer,
    FindLayerInspectionFacts? Inspection);

internal sealed record FindSourceLayerFacts(
    SourceLogicalSource Source,
    string? Description,
    IReadOnlyList<FindLayerFacts> Layers,
    IReadOnlyList<FindFinding> Findings,
    FindCoverageState Coverage);
