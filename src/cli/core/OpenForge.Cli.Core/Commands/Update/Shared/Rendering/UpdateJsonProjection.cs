using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Update.Models.Presentation;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Rendering;

internal static partial class UpdateJsonProjection
{
    internal static UpdateJsonDocument Create(UpdateResult result)
    {
        CliOperationStage.ValidateResult(result);
        return new UpdateJsonDocument
        {
            SchemaVersion = UpdateDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CliStatusDefinitions.Read(result.Status).MachineName,
            Workspace = result.Workspace is null ? null : Workspace(result.Workspace),
            Result = new UpdateJsonResult
            {
                Mode = UpdateDefinitions.ReadMachineName(result.Mode),
                Force = result.Force,
                Prune = result.Prune,
                Automatic = result.Automatic,
                Source = result.Source is null ? null : Source(result.Source),
                Comparisons = result.Comparisons.Select(Comparison).ToArray(),
                GeneratedNavigation = result.GeneratedNavigation is null
                    ? null
                    : GeneratedNavigation(result.GeneratedNavigation),
                Effects = result.Effects.Select(Effect).ToArray(),
                Lifecycle = Lifecycle(result.Lifecycle),
                Recovery = Recovery(result.Recovery),
                Verification = UpdateDefinitions.ReadMachineName(result.Verification),
                Findings = result.Findings.Select(Finding).ToArray(),
            },
            Next = result.Next is null
                ? null
                : new UpdateJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
    }

    private static UpdateJsonWorkspace Workspace(CliWorkspace workspace)
        => new()
        {
            Path = workspace.LexicalRoot,
            SelectedBy = WorkspaceSelectionWireVocabulary.Read(workspace.SelectedBy),
        };
}
