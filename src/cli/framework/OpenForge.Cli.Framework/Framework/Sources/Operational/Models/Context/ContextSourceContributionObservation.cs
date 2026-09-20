namespace OpenForge.Cli.Core.Framework.Sources.Operational.Models.Context;

internal sealed record ContextSourceContributionObservation
{
    public required string SourceId { get; init; }

    public required long Utf8Bytes { get; init; }

    public required IReadOnlyList<ContextLayerContributionObservation> Layers { get; init; }
}
