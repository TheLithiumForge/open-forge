namespace OpenForge.Cli.Core.Commands.Status.Models.Presentation;

internal sealed class StatusJsonContext
{
    public required string TokenEstimator { get; init; }

    public required StatusJsonStartupComparison Startup { get; init; }

    public required StatusJsonMeasurement TotalAvailable { get; init; }

    public required StatusJsonDecimalValue StartupPercentage { get; init; }

    public required StatusJsonMeasurement Continuity { get; init; }

    public required StatusJsonContinuitySource[] ContinuitySources { get; init; }
}
internal sealed class StatusJsonStartupComparison
{
    public required StatusJsonMeasurement Initial { get; init; }

    public required StatusJsonMeasurement Current { get; init; }

    public required StatusJsonMeasurement Difference { get; init; }
}

internal sealed class StatusJsonMeasurement
{
    public required StatusJsonIntegerValue Files { get; init; }

    public required StatusJsonIntegerValue Characters { get; init; }

    public required StatusJsonIntegerValue Utf8Bytes { get; init; }

    public required StatusJsonIntegerValue EstimatedTokens { get; init; }
}

internal sealed class StatusJsonIntegerValue
{
    public required string State { get; init; }

    public required long? Value { get; init; }
}

internal sealed class StatusJsonDecimalValue
{
    public required string State { get; init; }

    public required decimal? Value { get; init; }
}

internal sealed class StatusJsonContinuitySource
{
    public required string SourceId { get; init; }

    public required long Utf8Bytes { get; init; }

    public required StatusJsonContextLayer[] Layers { get; init; }
}

internal sealed class StatusJsonContextLayer
{
    public required string Path { get; init; }

    public required long Utf8Bytes { get; init; }
}
