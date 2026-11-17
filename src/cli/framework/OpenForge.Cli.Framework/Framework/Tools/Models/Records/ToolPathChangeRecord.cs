namespace OpenForge.Cli.Core.Framework.Tools.Models.Records;

internal enum ToolPathChangeKind { WindowsUserPath, ShellBlock }

internal sealed record ToolPathChangeRecord
{
    public required ToolPathChangeKind Kind { get; init; }
    public required string Target { get; init; }
    public string? StartMarker { get; init; }
    public string? EndMarker { get; init; }
    public required ToolRecordedValue Prior { get; init; }
    public required ToolRecordedValue Applied { get; init; }
}

internal sealed record ToolPathState
{
    public required int SchemaVersion { get; init; }
    public required string BinPath { get; init; }
    public required IReadOnlyList<ToolPathChangeRecord> Changes { get; init; }
}
