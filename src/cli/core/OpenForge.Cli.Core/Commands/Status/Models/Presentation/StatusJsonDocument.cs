namespace OpenForge.Cli.Core.Commands.Status.Models.Presentation;

internal sealed class StatusJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required StatusJsonWorkspace? Workspace { get; init; }

    public required StatusJsonResult Result { get; init; }

    public required StatusJsonNext? Next { get; init; }
}
internal sealed class StatusJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed class StatusJsonResult
{
    public required StatusJsonInstallation Installation { get; init; }

    public required StatusJsonContext Context { get; init; }

    public required StatusJsonStructure Structure { get; init; }

    public required StatusJsonLifecycle Lifecycle { get; init; }

    public required StatusJsonLibrary Library { get; init; }

    public required StatusJsonRecovery Recovery { get; init; }

    public required StatusJsonFinding[] Findings { get; init; }
}

internal sealed class StatusJsonInstallation
{
    public required string State { get; init; }

    public required string? EntryPath { get; init; }

    public required string? LoaderPath { get; init; }
}

internal sealed class StatusJsonFinding
{
    public required string Code { get; init; }

    public required string Status { get; init; }

    public required string? Subject { get; init; }

    public required string Cause { get; init; }
}

internal sealed class StatusJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}
