using OpenForge.Cli.Core.Commands.Status.Models.Presentation;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static class StatusJsonProjection
{
    internal static StatusJsonDocument Create(StatusResult result)
        => new()
        {
            SchemaVersion = StatusDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CliStatusDefinitions.Read(result.Status).MachineName,
            Workspace = result.Workspace is { } workspace
                ? new StatusJsonWorkspace
                {
                    Path = workspace.LexicalRoot,
                    SelectedBy = StatusWireVocabulary.WorkspaceSelection(workspace.SelectedBy),
                }
                : null,
            Result = new StatusJsonResult
            {
                Installation = new StatusJsonInstallation
                {
                    State = StatusWireVocabulary.InstallationState(result.Facts.Installation.State),
                    EntryPath = result.Facts.Installation.EntryPath,
                    LoaderPath = result.Facts.Installation.LoaderPath,
                },
                Context = StatusJsonContextProjection.Create(result.Facts.Context),
                Structure = StatusJsonStructureProjection.Create(result.Facts.Structure),
                Lifecycle = StatusJsonLifecycleProjection.Create(result.Facts.Lifecycle),
                Recovery = StatusJsonRecoveryProjection.Create(result.Facts.Recovery),
                Findings = result.Findings.Select(Project).ToArray(),
            },
            Next = result.Next is { } next
                ? new StatusJsonNext { Command = next.Command, Reason = next.Reason }
                : null,
        };

    private static StatusJsonFinding Project(StatusFinding finding)
        => new()
        {
            Code = StatusDefinitions.ReadFindingCode(finding.Code),
            Status = CliStatusDefinitions.Read(finding.Status).MachineName,
            Subject = finding.Subject,
            Cause = finding.Cause,
        };
}
