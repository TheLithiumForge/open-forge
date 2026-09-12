namespace OpenForge.Cli.Core.Commands.Status.Models.Presentation;

internal sealed class StatusCompactJsonResult
{
    public required StatusJsonInstallation Installation { get; init; }

    public required StatusCompactJsonContext Context { get; init; }

    public required StatusJsonStructure Structure { get; init; }

    public required StatusJsonLifecycle Lifecycle { get; init; }

    public required StatusJsonLibrary Library { get; init; }

    public required StatusJsonRecovery Recovery { get; init; }

    public required StatusJsonFinding[] Findings { get; init; }
}

internal sealed class StatusCompactJsonContext
{
    public required string TokenEstimator { get; init; }

    public required StatusJsonStartupComparison Startup { get; init; }

    public required StatusJsonMeasurement TotalAvailable { get; init; }

    public required StatusJsonDecimalValue StartupPercentage { get; init; }

    public required StatusJsonMeasurement Continuity { get; init; }
}
