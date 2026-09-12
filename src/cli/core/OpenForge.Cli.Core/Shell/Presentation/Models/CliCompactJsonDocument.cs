using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Presentation.Models;

internal sealed class CliCompactJsonDocument<TResult> where TResult : class
{
    public int SchemaVersion => 2;
    public string View => CliPresentationDefinitions.Compact;
    public required string Command { get; init; }
    public required string Status { get; init; }
    public required CliCompactJsonWorkspace? Workspace { get; init; }
    public required TResult Result { get; init; }
    public required CliCompactJsonNext? Next { get; init; }
}

internal sealed record CliCompactJsonWorkspace(string Path, string SelectedBy);
internal sealed record CliCompactJsonNext(string Command, string Reason);
