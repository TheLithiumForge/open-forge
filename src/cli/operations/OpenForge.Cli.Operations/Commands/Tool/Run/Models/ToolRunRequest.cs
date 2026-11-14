namespace OpenForge.Cli.Core.Commands.Tool.Run.Models;

internal sealed record ToolRunRequest
{
    internal const string HostProjectDirectoryVariable = "CLAUDE_PROJECT_DIR";

    public required string? WorkspacePath { get; init; }
    public required string CurrentDirectory { get; init; }
    public required string? HostProjectDirectory { get; init; }
    public required string Id { get; init; }
    public required string[] Arguments { get; init; }
    public required bool Interactive { get; init; }
}
