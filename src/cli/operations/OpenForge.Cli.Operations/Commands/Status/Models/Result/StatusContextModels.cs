namespace OpenForge.Cli.Core.Commands.Status.Models.Result;

internal sealed record StatusIntegerValue(
    StatusValueState State,
    long? Value);

internal sealed record StatusDecimalValue(
    StatusValueState State,
    decimal? Value);

internal sealed record StatusMeasurement(
    StatusIntegerValue Files,
    StatusIntegerValue Characters,
    StatusIntegerValue Utf8Bytes,
    StatusIntegerValue EstimatedTokens);

internal sealed record StatusStartupComparison(
    StatusMeasurement Initial,
    StatusMeasurement Current,
    StatusMeasurement Difference);

internal sealed record StatusContextLayer(
    string Path,
    long Utf8Bytes);

internal sealed record StatusContinuitySource
{
    public required string SourceId { get; init; }

    public required long Utf8Bytes { get; init; }

    public required IReadOnlyList<StatusContextLayer> Layers { get; init; }
}
internal sealed record StatusContext
{
    public required string TokenEstimator { get; init; }

    public required StatusStartupComparison Startup { get; init; }

    public required StatusMeasurement TotalAvailable { get; init; }

    public required StatusDecimalValue StartupPercentage { get; init; }

    public required StatusMeasurement Continuity { get; init; }

    public required IReadOnlyList<StatusContinuitySource> ContinuitySources { get; init; }
}
