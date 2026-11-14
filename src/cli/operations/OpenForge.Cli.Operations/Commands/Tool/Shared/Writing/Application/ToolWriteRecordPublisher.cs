using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;

internal sealed class ToolWriteRecordPublisher(IToolRecordService records)
{
    internal Task<ToolRecordsResult> ReadAsync(ToolRecordsRequest locations, CancellationToken cancellationToken)
        => records.ReadAsync(locations, cancellationToken);

    internal async ValueTask<bool> AddGrantsAsync(
        ToolWriteExecution state, ToolRecordGrantRequest request, CancellationToken cancellationToken)
    {
        var written = await records.AddAllowInstallPathsAsync(request, cancellationToken).ConfigureAwait(false);
        state.Backups.AddRange(written.PendingBackups);
        var complete = written.Failure is null && written.Unfinished.Count == 0;
        state.Grants = new(request.AllowInstallPaths, complete ? ToolWriteStages.Completed : ToolWriteStages.Failed,
            written.WrittenPaths, written.Unfinished);
        if (!complete)
        {
            state.Findings.Add(new(ToolWriteFindingCode.RecordFailed, CliSemanticStatus.Failed, Cause: written.Failure));
            state.Stopped = true;
            return false;
        }
        state.Records = await records.ReadAsync(request.Locations, cancellationToken).ConfigureAwait(false);
        return true;
    }

    internal async ValueTask<bool> PublishAuxiliaryAsync(
        ToolWriteExecution state, ToolWritePlan plan, ToolPathState? path,
        IReadOnlyList<string> removed, CancellationToken cancellationToken)
    {
        var locations = new ToolRecordsRequest(plan.Input.Request.Workspace.LexicalRoot, plan.Input.Facts.HomePath);
        var result = await records.WriteAsync(new()
        {
            Locations = locations,
            Expected = state.Records,
            Portable = state.Records.Portable ?? new()
            {
                Settings = new Dictionary<string, ToolSetting>(),
                Lock = new Dictionary<string, ToolLockRecord>(),
            },
            Machine = [.. state.Records.Machine.Values.Where(machine => !removed.Contains(machine.CommandPath, StringComparer.Ordinal))],
            Runtimes = [.. state.Records.Runtimes.Values.Where(runtime => !removed.Contains(runtime.RuntimePath, StringComparer.Ordinal))],
            WorkspaceMachine = state.Records.WorkspaceMachine,
            Path = path ?? state.Records.Path,
        }, cancellationToken).ConfigureAwait(false);
        state.Backups.AddRange(result.PendingBackups);
        if (result.Failure is not null || result.Unfinished.Count != 0)
        {
            state.Findings.Add(new(ToolWriteFindingCode.RecordFailed, CliSemanticStatus.Failed, Cause: result.Failure));
            state.Stopped = true;
            return false;
        }
        state.Records = await records.ReadAsync(locations, cancellationToken).ConfigureAwait(false);
        if (!Readable(state.Records))
        {
            state.Findings.Add(new(ToolWriteFindingCode.RecordFailed, CliSemanticStatus.Failed));
            state.Stopped = true;
            return false;
        }
        return true;
    }

    internal async ValueTask<bool> PublishAsync(
        ToolWriteExecution state, ToolWritePublication publication, CancellationToken cancellationToken)
    {
        var plan = publication.Plan;
        var id = publication.Tool.Entry.Identity.Id;
        var portable = state.Records.Portable ?? new ToolPortableRecords
        {
            Settings = new Dictionary<string, ToolSetting>(),
            Lock = new Dictionary<string, ToolLockRecord>(),
        };
        var settings = portable.Settings.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        var locked = portable.Lock.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        if (publication.RemoveDeclaration) settings.Remove(id);
        else if (publication.Tool.Profile is { } profile) settings[id] = new(profile.Range);
        if (publication.Lock is { } receipt) locked[id] = receipt;
        else locked.Remove(id);
        var machine = state.Records.Machine.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        if (publication.Machine is { } installed) machine[id] = installed;
        var runtimes = state.Records.Runtimes.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        foreach (var runtime in publication.Runtimes) runtimes[runtime.Version] = runtime;
        var workspaceState = state.Records.WorkspaceMachine;
        if (publication.NativeSettings is { } native)
        {
            workspaceState ??= new()
            {
                SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
                WorkspaceKey = WorkspaceIdentity.Key(plan.Input.Request.Workspace.PhysicalRoot),
                WorkspacePath = plan.Input.Request.Workspace.PhysicalRoot,
                NativeSettings = [],
            };
            workspaceState = workspaceState with
            {
                NativeSettings = [.. workspaceState.NativeSettings.Where(setting => setting.ToolId != id), .. native],
            };
        }
        var locations = new ToolRecordsRequest(plan.Input.Request.Workspace.LexicalRoot, plan.Input.Facts.HomePath);
        var written = await records.WriteAsync(new()
        {
            Locations = locations,
            Expected = state.Records,
            Portable = new() { Settings = settings, Lock = locked },
            Machine = [.. machine.Values.OrderBy(value => value.ToolId, StringComparer.Ordinal)],
            Runtimes = [.. runtimes.Values.OrderBy(value => value.Version, StringComparer.Ordinal)],
            WorkspaceMachine = workspaceState,
            Path = state.Records.Path,
        }, cancellationToken).ConfigureAwait(false);
        state.Backups.AddRange(written.PendingBackups);
        state.Outcomes.Add(new(id, ToolWriteStages.Records, written.Failure is null && written.Unfinished.Count == 0
            ? ToolWriteStages.Completed : ToolWriteStages.Failed, written.WrittenPaths, written.Unfinished));
        if (written.Failure is not null || written.Unfinished.Count != 0)
        {
            state.Findings.Add(new(ToolWriteFindingCode.RecordFailed, CliSemanticStatus.Failed, id, Cause: written.Failure));
            state.Stopped = true;
            return false;
        }
        var fresh = await records.ReadAsync(locations, cancellationToken).ConfigureAwait(false);
        if (!Readable(fresh))
        {
            state.Findings.Add(new(ToolWriteFindingCode.RecordFailed, CliSemanticStatus.Failed, id));
            state.Stopped = true;
            return false;
        }
        state.Records = fresh;
        return true;
    }

    private static bool Readable(ToolRecordsResult result)
        => result.PortableState == ToolRecordReadState.Available
            && result.MachineState is not (ToolRecordReadState.Invalid or ToolRecordReadState.Unavailable)
            && result.WorkspaceMachineState is not (ToolRecordReadState.Invalid or ToolRecordReadState.Unavailable)
            && result.RuntimeRecordsState is not (ToolRecordReadState.Invalid or ToolRecordReadState.Unavailable)
            && result.PathRecordState is not (ToolRecordReadState.Invalid or ToolRecordReadState.Unavailable);
}
