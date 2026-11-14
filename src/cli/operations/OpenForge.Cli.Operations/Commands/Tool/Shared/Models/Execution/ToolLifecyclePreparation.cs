using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Execution;

internal sealed record ToolExecutableLocation(string Root, string Entry, string Sha256);

internal sealed record ToolCommandValues
{
    public required string Worktree { get; init; }
    public required string CodeRoot { get; init; }
    public required string Target { get; init; }
    public string? State { get; init; }
    public string? Cache { get; init; }
    public string? HookSink { get; init; }
    public string? ProjectId { get; init; }
    public string? IndexDirName { get; init; }
}

internal sealed record ToolLifecyclePreparation
{
    public required ToolLifecycle Lifecycle { get; init; }
    public required string Action { get; init; }
    public required ToolExecutableLocation Executable { get; init; }
    public required ToolCommandValues Values { get; init; }
    public required string[] Hosts { get; init; }
    public required Dictionary<string, string> Environment { get; init; }
}
