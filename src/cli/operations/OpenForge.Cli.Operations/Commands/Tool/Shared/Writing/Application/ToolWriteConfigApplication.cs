using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;

internal sealed class ToolWriteConfigApplication(IToolConfigService config, ToolWriteRecordPublisher publisher)
{
    internal async ValueTask<bool> ApplyAsync(
        ToolWritePlan plan, ToolWriteToolPlan tool, ToolWriteExecution state, CancellationToken cancellationToken)
    {
        var id = tool.Entry.Identity.Id;
        if (tool.Config is not { } original) return false;
        var current = state.Records.Portable?.Lock.GetValueOrDefault(id)
            ?? throw new InvalidOperationException("A config application needs its published tool lock.");
        var native = NativeSettings(state, id);
        var request = original.Request with { AppliedChanges = current.Changes, NativeSettings = native };
        var replanned = await config.PlanAsync(request, cancellationToken).ConfigureAwait(false);
        replanned = replanned with
        {
            Changes = [.. replanned.Changes.Select(change => change.Change is ToolNativeConfigChange
                ? original.Changes.Single(initial => initial.Change.Id == change.Change.Id) : change)],
        };
        if (replanned.Changes.Any(change => change.Disposition == ToolChangeDisposition.Conflict)
            || replanned.Removals.Any(change => change.Disposition == ToolChangeDisposition.Conflict))
            return Fail(ToolWriteFindingCode.ConfigConflict, null);

        foreach (var removal in replanned.Removals)
        {
            current = state.Records.Portable?.Lock.GetValueOrDefault(id) ?? current;
            var removed = await config.RemoveAsync(new(new()
            {
                ToolId = id,
                Context = request.Context,
                Lock = current,
                NativeSettings = NativeSettings(state, id),
            }, [removal]), cancellationToken).ConfigureAwait(false);
            state.Backups.AddRange(removed.PendingBackups);
            current = current with { Changes = removed.RemainingAdditions };
            state.Outcomes.Add(new(id, ToolWriteStages.Config, removed.Failure is null ? ToolWriteStages.Completed : ToolWriteStages.Failed,
                removed.RemovedChanges, removed.Unfinished));
            if (!await PublishAsync(current, removed.KeptNativeSettings).ConfigureAwait(false)) return false;
            if (removed.Failure is not null || removed.Unfinished.Count != 0) return Fail(ToolWriteFindingCode.ConfigFailed, removed.Failure);
        }
        foreach (var observation in replanned.Changes)
        {
            current = state.Records.Portable?.Lock.GetValueOrDefault(id) ?? current;
            var staged = new ToolConfigPlan(replanned.Request with
            {
                AppliedChanges = current.Changes,
                NativeSettings = NativeSettings(state, id),
            }, [observation], []);
            var applied = await config.ApplyAsync(staged, cancellationToken).ConfigureAwait(false);
            state.Backups.AddRange(applied.PendingBackups);
            current = current with { Changes = applied.RemainingAdditions };
            state.Outcomes.Add(new(id, ToolWriteStages.Config, applied.Failure is null ? ToolWriteStages.Completed : ToolWriteStages.Failed,
                [.. applied.CompletedChanges, .. applied.RemovedChanges], applied.Unfinished));
            if (!await PublishAsync(current, applied.NativeSettings).ConfigureAwait(false)) return false;
            if (applied.Failure is not null || applied.Unfinished.Count != 0) return Fail(ToolWriteFindingCode.ConfigFailed, applied.Failure);
        }
        current = current with { ProfileDigest = tool.Digest };
        return await PublishAsync(current, NativeSettings(state, id)).ConfigureAwait(false);

        ValueTask<bool> PublishAsync(ToolLockRecord receipt, IReadOnlyList<ToolNativeSettingRecord> settings)
            => publisher.PublishAsync(state, new()
            {
                Plan = plan,
                Tool = tool,
                Lock = receipt,
                NativeSettings = settings,
            }, cancellationToken);
        bool Fail(ToolWriteFindingCode code, string? cause)
        {
            state.Findings.Add(new(code, CliSemanticStatus.Failed, id, Cause: cause));
            state.Stopped = true;
            return false;
        }
    }

    internal async ValueTask<bool> RemoveAsync(
        ToolWritePlan plan, ToolWriteToolPlan tool, ToolWriteExecution state, CancellationToken cancellationToken)
    {
        var id = tool.Entry.Identity.Id;
        if (tool.Removal is not { } original) return false;
        var current = state.Records.Portable?.Lock.GetValueOrDefault(id) ?? original.Request.Lock;
        foreach (var observation in original.Changes)
        {
            var result = await config.RemoveAsync(new(original.Request with
            {
                Lock = current,
                NativeSettings = NativeSettings(state, id),
            }, [observation]), cancellationToken).ConfigureAwait(false);
            state.Backups.AddRange(result.PendingBackups);
            current = current with { Changes = result.RemainingAdditions };
            var complete = !current.Changes.Any(change => change is not ToolNativeCommandAddition);
            state.Outcomes.Add(new(id, ToolWriteStages.Config, result.Failure is null ? ToolWriteStages.Completed : ToolWriteStages.Failed,
                result.RemovedChanges, result.Unfinished));
            if (!await publisher.PublishAsync(state, new()
            {
                Plan = plan,
                Tool = tool,
                Lock = current.Changes.Count == 0 ? null : current,
                NativeSettings = result.KeptNativeSettings,
                RemoveDeclaration = complete,
            }, cancellationToken).ConfigureAwait(false)) return false;
            if (result.Failure is not null)
            {
                state.Findings.Add(new(ToolWriteFindingCode.ConfigFailed, CliSemanticStatus.Failed, id, Cause: result.Failure));
                state.Stopped = true;
                return false;
            }
            if (observation.Disposition == ToolChangeDisposition.Conflict || result.Unfinished.Count != 0)
                state.Findings.Add(new(ToolWriteFindingCode.ConfigConflict, CliSemanticStatus.Incomplete, id, Cause: observation.Conflict));
        }
        var finished = !current.Changes.Any(change => change is not ToolNativeCommandAddition);
        return await publisher.PublishAsync(state, new()
        {
            Plan = plan,
            Tool = tool,
            Lock = current.Changes.Count == 0 ? null : current,
            NativeSettings = NativeSettings(state, id),
            RemoveDeclaration = finished,
        }, cancellationToken).ConfigureAwait(false);
    }

    private static IReadOnlyList<ToolNativeSettingRecord> NativeSettings(ToolWriteExecution state, string id)
        => [.. (state.Records.WorkspaceMachine?.NativeSettings ?? []).Where(setting => setting.ToolId == id)];
}
