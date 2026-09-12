using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

internal static class CliCompactJsonProjection
{
    internal static CliCompactJsonDocument<TResult> Create<TResult>(ICliCommandResult operation, TResult result)
        where TResult : class
        => new()
        {
            Command = operation.Command,
            Status = CliStatusDefinitions.Read(operation.Status).MachineName,
            Workspace = operation.Workspace is { } workspace
                ? new CliCompactJsonWorkspace(Path: workspace.LexicalRoot, SelectedBy: workspace.SelectedBy switch
                {
                    CliWorkspaceSelectionMethod.CurrentDirectory => "current-directory",
                    CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
                    _ => throw new ArgumentOutOfRangeException(nameof(operation), workspace.SelectedBy, "The workspace selection method is not defined."),
                })
                : null,
            Result = result,
            Next = operation.Next is { } next ? new CliCompactJsonNext(Command: next.Command, Reason: next.Reason) : null,
        };
}
