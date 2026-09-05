namespace OpenForge.Cli.Core.Commands.Doctor.Models.Result;

internal enum DoctorNextActionKind
{
    RepairPreview,
    AcceptedOperation,
    FutureOperation,
    ReviewCandidates,
    ManualDecision,
}

internal enum DoctorNextOperation
{
    Repair,
    Index,
    Cleanup,
    Install,
    Update,
    ExtensionCreate,
    ExtensionInstall,
    ExtensionUpdate,
    ExtensionRemove,
}

internal sealed record DoctorNextAction
{
    public required DoctorNextActionKind Kind { get; init; }

    public required DoctorNextOperation? Operation { get; init; }

    public required string? Command { get; init; }

    public required string Reason { get; init; }
}
