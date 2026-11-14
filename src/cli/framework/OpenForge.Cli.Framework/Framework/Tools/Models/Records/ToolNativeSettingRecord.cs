namespace OpenForge.Cli.Core.Framework.Tools.Models.Records;

internal sealed record ToolNativeSettingRecord
{
    public required string ToolId { get; init; }
    public required string ChangeId { get; init; }
    public required string NativeHomePath { get; init; }
    public required ToolRecordedValue Previous { get; init; }
    public required ToolRecordedValue Applied { get; init; }
    public required IReadOnlyList<string> ManualRestoreArguments { get; init; }
}

internal sealed record ToolWorkspaceMachineState
{
    public required int SchemaVersion { get; init; }
    public required string WorkspaceKey { get; init; }
    public required string WorkspacePath { get; init; }
    public required IReadOnlyList<ToolNativeSettingRecord> NativeSettings { get; init; }
}
