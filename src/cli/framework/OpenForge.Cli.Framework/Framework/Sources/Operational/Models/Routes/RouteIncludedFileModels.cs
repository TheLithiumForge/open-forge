using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Context;

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
    public required IReadOnlyList<RouteIncludedFileObservation> Files { get; init; }
    public required int HandWrittenRows { get; init; }
    public required ContextMeasurementObservation Startup { get; init; }
    public required ContextMeasurementObservation Total { get; init; }
    public required ContextMeasurementObservation Continuity { get; init; }
    public required IReadOnlyList<ContextSourceContributionObservation> ContinuitySources { get; init; }
    public required IReadOnlyList<string> Pending { get; init; }
}
