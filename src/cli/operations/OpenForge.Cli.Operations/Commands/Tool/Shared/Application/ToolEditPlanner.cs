using OpenForge.Cli.Core.Commands.Tool.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models;
using OpenForge.Cli.Core.Framework.Tools.Shared.Integration;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Application;

/// <summary>The edits to apply, the changes to report for them and a conflict that stops the command.</summary>
internal sealed record ToolEditPlan(IReadOnlyList<ToolFileEdit> Edits, IReadOnlyList<ToolWriteChange> Changes, ToolFinding? Conflict);

/// <summary>Plans Open Forge's own Tool edits from the current files. Each plan is only as fresh as the read it came from.</summary>
internal static class ToolEditPlanner
{
    internal static async ValueTask<ToolEditPlan> PlanAsync(
        ToolEntry entry, CliWorkspace workspace, bool removal, CancellationToken cancellationToken)
    {
        var settings = await ToolObserver.ReadSettingsAsync(workspace, cancellationToken).ConfigureAwait(false);
        if (!settings.Usable || settings.Read.Snapshot is not { } settingsSnapshot)
        {
            return new([], [], new(ToolFindingCode.SettingsUnavailable, CliSemanticStatus.Blocked, entry.Id, settings.Read.Cause)
            {
                Path = WorkspaceSettingsDefinitions.RelativePath,
            });
        }
        // The tools list lives in Open Forge's settings, so a folder without Open Forge has nowhere to record it.
        if (!removal && settingsSnapshot.Kind == FileExpectationKind.Missing
            && !Directory.Exists(Path.Combine(workspace.LexicalRoot, WorkspaceSettingsDefinitions.DirectoryName)))
            return new([], [], new(ToolFindingCode.OpenForgeMissing, CliSemanticStatus.Blocked, entry.Id));
        var edits = new List<ToolFileEdit>();
        var changes = new List<ToolWriteChange>();
        foreach (var view in await ToolObserver.ReadIntegrationsAsync(entry, workspace, settings.ArchivePath, cancellationToken).ConfigureAwait(false))
        {
            if (view.State == ToolIntegrationState.Conflict || view.File.Snapshot is not { } snapshot)
                return new([], [], new(ToolFindingCode.IntegrationConflict, CliSemanticStatus.Blocked, entry.Id, view.Cause) { Path = view.Integration.Path });
            if (view.State == (removal ? ToolIntegrationState.Absent : ToolIntegrationState.Present)) continue;
            var intended = removal
                ? ToolIntegrationEditor.Remove(view.Integration, view.File.Bytes ?? [], settings.ArchivePath)
                : ToolIntegrationEditor.Add(view.Integration, view.File.Bytes, settings.ArchivePath);
            if (ToolFileReader.Plan(snapshot, intended) is not { } change) continue;
            edits.Add(new(change, snapshot));
            changes.Add(new() { Path = view.Integration.Path, Kind = ToolWire.Integration, Purpose = view.Integration.Purpose, Outcome = ToolWire.Planned });
        }
        var bytes = settingsSnapshot.Bytes.AsMemory();
        var settingsBytes = removal ? WorkspaceSettingsCodec.RemoveTool(bytes, entry.Id) : WorkspaceSettingsCodec.AddTool(bytes, entry.Id);
        if (settingsBytes is not null && ToolFileReader.Plan(settingsSnapshot, settingsBytes) is { } settingsChange)
        {
            edits.Add(new(settingsChange, settingsSnapshot));
            changes.Add(new()
            {
                Path = WorkspaceSettingsDefinitions.RelativePath,
                Kind = removal ? ToolWire.RemoveTool : ToolWire.AddTool,
                Outcome = ToolWire.Planned,
            });
        }
        return new(edits, changes, null);
    }
}
