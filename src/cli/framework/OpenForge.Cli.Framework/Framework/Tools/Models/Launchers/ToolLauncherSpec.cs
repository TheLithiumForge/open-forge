namespace OpenForge.Cli.Core.Framework.Tools.Models.Launchers;

internal enum ToolLauncherKind { WindowsCmd, PosixSh, NativeExecutable }
internal enum ToolLauncherTargetKind { RuntimeEntry, ReleaseLauncher, NativeBinary }

internal sealed record ToolLauncherSpec
{
    public required string ToolId { get; init; }
    public required ToolLauncherKind Kind { get; init; }
    public required ToolLauncherTargetKind TargetKind { get; init; }
    public required string LauncherPath { get; init; }
    public string? RuntimePath { get; init; }
    public required string EntryPath { get; init; }
    public required IReadOnlyList<string> Arguments { get; init; }
}
