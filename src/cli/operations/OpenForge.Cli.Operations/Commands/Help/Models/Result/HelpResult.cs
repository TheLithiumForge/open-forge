using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Help.Models.Result;

internal sealed record HelpResult : ICliCommandResult
{
    public string Command => HelpDefinitions.CommandIdentity;
    public required CliSemanticStatus Status { get; init; }
    public CliWorkspace? Workspace => null;
    public CliNextAction? Next { get; init; }
    internal required string? Topic { get; init; }
    internal IReadOnlyList<string> Topics { get; init; } = HelpDefinitions.Topics;
    internal string? Authoring { get; init; }
    internal string? Cause { get; init; }
    internal string? SuggestedCommand { get; init; }

    internal string? FindingCode => Status switch
    {
        CliSemanticStatus.Complete => null,
        CliSemanticStatus.Invalid => "help.invalid-input",
        CliSemanticStatus.Failed => "help.content-unavailable",
        CliSemanticStatus.Interrupted => "help.interrupted",
        _ => throw new ArgumentOutOfRangeException(nameof(Status), Status, "The Help status is not defined."),
    };
}
