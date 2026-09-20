namespace OpenForge.Cli.Core.Commands.References.Models.Presentation;

internal sealed class ReferencesJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required ReferencesJsonWorkspace? Workspace { get; init; }

    public required ReferencesJsonResult Result { get; init; }

    public required ReferencesJsonNext? Next { get; init; }
}

internal sealed class ReferencesJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed class ReferencesJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}
