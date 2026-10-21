using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Include.Models.Result;

internal sealed record RouteIncludeResult : ICliCommandResult
{
    public string Command => RouteIncludeDefinitions.CommandIdentity;
    public CliSemanticStatus Status => Outcome.Status;
    public required CliWorkspace? Workspace { get; init; }
    public CliNextAction? Next => Status switch
    {
        CliSemanticStatus.Complete => null,
        CliSemanticStatus.Invalid => new("open-forge route include --help", "Correct the named input and retry."),
        CliSemanticStatus.Attention => new("open-forge cleanup", "Review the retained recovery artifact."),
        _ => new("open-forge route include --help", "Review the reported cause and retry from a fresh preview."),
    };
    internal string? WorkspacePath => Workspace?.LexicalRoot;
    internal bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;
    internal required string Requested { get; init; }
    internal bool DryRun { get; init; }
    internal required RouteInclusionOutcome Outcome { get; init; }
}
