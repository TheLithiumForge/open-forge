namespace OpenForge.Cli.Core.Commands.Doctor.Models.Presentation;

internal sealed class DoctorJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required DoctorJsonWorkspace? Workspace { get; init; }

    public required DoctorJsonResult Result { get; init; }

    public required DoctorJsonEnvelopeNext? Next { get; init; }
}

internal sealed class DoctorJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed class DoctorJsonEnvelopeNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}
