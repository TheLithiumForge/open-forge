using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Context;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;

internal sealed record RouteIncludedFileObservation(
    string Path,
    bool Registered,
    bool Listed,
    bool FileExists,
    bool Missing,
    string? Rejection);

internal sealed record RouteIncludedFilesObservation
{
    public IReadOnlyList<SourceIncludedEntryRow> Rows { get; init; } = [];
    public IReadOnlyList<RouteIncludedLayerObservation> Layers { get; init; } = [];
    public required IReadOnlyList<RouteIncludedFileObservation> Files { get; init; }
    public required int HandWrittenRows { get; init; }
    public required ContextMeasurementObservation Startup { get; init; }
    public required ContextMeasurementObservation Total { get; init; }
    public required ContextMeasurementObservation Continuity { get; init; }
    public required IReadOnlyList<ContextSourceContributionObservation> ContinuitySources { get; init; }
    public required IReadOnlyList<string> Pending { get; init; }
}

internal sealed record RouteIncludedLayerObservation(
    string PhysicalPath,
    RouteContextSource Source,
    bool Startup,
    bool Continuity);
