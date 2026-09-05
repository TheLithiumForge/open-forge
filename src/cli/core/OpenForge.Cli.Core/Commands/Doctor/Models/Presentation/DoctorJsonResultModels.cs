namespace OpenForge.Cli.Core.Commands.Doctor.Models.Presentation;

internal sealed class DoctorJsonResult
{
    public required bool ReadOnly { get; init; }

    public required bool ChangesMade { get; init; }

    public required string Coverage { get; init; }

    public required DoctorJsonCounts Counts { get; init; }

    public required DoctorJsonAction[] Actions { get; init; }

    public required DoctorJsonDomain[] Domains { get; init; }
}

internal sealed class DoctorJsonCounts
{
    public required DoctorJsonResolutionCounts Resolution { get; init; }

    public required DoctorJsonSeverityCounts Severity { get; init; }
}

internal sealed class DoctorJsonResolutionCounts
{
    public required DoctorJsonCount SafeExact { get; init; }

    public required DoctorJsonCount GuidedChoice { get; init; }

    public required DoctorJsonCount TargetedOperation { get; init; }

    public required DoctorJsonCount ManualDecision { get; init; }

    public required DoctorJsonCount BlockedRepair { get; init; }

    public required DoctorJsonCount Informational { get; init; }
}

internal sealed class DoctorJsonSeverityCounts
{
    public required DoctorJsonCount Information { get; init; }

    public required DoctorJsonCount Warning { get; init; }

    public required DoctorJsonCount Error { get; init; }
}

internal sealed class DoctorJsonCount
{
    public required string State { get; init; }

    public required long? Value { get; init; }
}
