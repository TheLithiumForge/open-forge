using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Tools.Shared.NativeSettings;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;

internal sealed partial class ToolConfigService
{
    public async Task<ToolConfigApplyResult> ApplyAsync(ToolConfigPlan plan, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ValidateIds(plan.Request);
        var remaining = plan.Request.AppliedChanges.ToList();
        var nativeSettings = plan.Request.NativeSettings.Where(fact => fact.ToolId == plan.Request.ToolId).ToList();
        var completed = new List<string>();
        var removed = new List<string>();
        var pending = new Dictionary<string, ToolBackupCompletionRequest>(ToolNativePaths.Comparer);
        string? failure = null;
        try
        {
            ToolRecipeExpansion.Validate(plan.Request.Context);
            ValidateRemovalSelection(plan.Removals, plan.Request.AppliedChanges);
            var conflict = plan.Removals.FirstOrDefault(change => change.Disposition == ToolChangeDisposition.Conflict)?.Conflict
                ?? plan.Changes.FirstOrDefault(change => change.Disposition == ToolChangeDisposition.Conflict)?.Conflict;
            if (plan.Removals.Any(change => change.Disposition == ToolChangeDisposition.Conflict) || plan.Changes.Any(change => change.Disposition == ToolChangeDisposition.Conflict))
                throw new InvalidDataException(conflict ?? "The configuration plan conflicts.");
            foreach (var removal in plan.Removals)
            {
                await ApplyRemovalAsync(plan.Request.Context, removal, pending, cancellationToken).ConfigureAwait(false);
                remaining.RemoveAll(receipt => receipt.Id == removal.Addition.Id);
                removed.Add(removal.Addition.Id);
            }
            var scope = ToolFileReader.Scope(plan.Request.Context.WorkspacePath);
            foreach (var observation in plan.Changes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!plan.Request.Changes.Any(change => change == observation.Change) || completed.Contains(observation.Change.Id, StringComparer.Ordinal))
                    throw new InvalidDataException("The planned change is duplicated or absent from its request.");
                ToolAppliedChange? addition;
                if (observation.Change is ToolNativeConfigChange native)
                {
                    if (!SameNative(observation.Desired, ToolNativeSettings.Value(native.DesiredValue))) throw new InvalidDataException("The native setting plan no longer matches its recipe.");
                    var current = await _native.ReadAsync(plan.Request, native, cancellationToken).ConfigureAwait(false);
                    if (!SameNative(current, observation.Current)) throw new IOException("The native setting changed after planning.");
                    var prior = NativeFact(plan.Request, native);
                    if (prior is not null && !SameNative(current, prior.Applied)) throw new IOException("The recorded native setting has drifted.");
                    var previous = prior?.Previous ?? ToolNativeSettings.Value(observation.Current.Native ?? throw new InvalidDataException("The previous native setting is unavailable."));
                    var manual = ManualArguments(plan.Request.Context, native, previous.Native ?? throw new InvalidDataException("The previous native setting is unavailable."));
                    var recorded = remaining.OfType<ToolNativeCommandAddition>().Any(receipt => receipt.Id == native.Id && receipt.AllowListId == native.AllowListId);
                    if (current.Native != native.DesiredValue || prior is null || !recorded) await _native.ApplyAsync(plan.Request, native, cancellationToken).ConfigureAwait(false);
                    var verified = await _native.ReadAsync(plan.Request, native, cancellationToken).ConfigureAwait(false);
                    if (verified.Native != native.DesiredValue) throw new IOException("The native setting write did not verify. Check the account setting manually.");
                    var fact = new ToolNativeSettingRecord
                    {
                        ToolId = plan.Request.ToolId,
                        ChangeId = native.Id,
                        NativeHomePath = ToolNativePaths.HomeIdentity(plan.Request.Context.NativeHomePath),
                        Previous = previous,
                        Applied = verified,
                        ManualRestoreArguments = manual
                    };
                    nativeSettings.RemoveAll(value => value.ChangeId == fact.ChangeId && !string.IsNullOrWhiteSpace(value.NativeHomePath)
                        && ToolNativePaths.Comparer.Equals(ToolNativePaths.HomeIdentity(value.NativeHomePath), fact.NativeHomePath));
                    nativeSettings.Add(fact);
                    addition = new ToolNativeCommandAddition { Id = native.Id, AllowListId = native.AllowListId };
                }
                else
                {
                    var resolved = ToolRecipeExpansion.Change(observation.Change, plan.Request.Context);
                    var target = ToolFileReader.Target(scope, ToolConfigEditor.PathOf(resolved));
                    var current = await ToolFileReader.ReadAsync(scope, target, cancellationToken).ConfigureAwait(false);
                    var before = current.HasBytes ? current.Bytes.ToArray() : null;
                    if (!Same(Bytes(before), observation.Current)) throw new IOException("The configuration file changed after planning.");
                    var edit = await EditAsync(plan.Request with { AppliedChanges = remaining }, resolved, before, cancellationToken).ConfigureAwait(false);
                    var conditionalSkip = resolved is ToolMarkedBlockChange { WhenNotIgnored: not null } && edit.Disposition == ToolChangeDisposition.NoChange;
                    if (!conditionalSkip && (!Same(Bytes(edit.Bytes), observation.Desired) || edit.Disposition != observation.Disposition))
                        throw new IOException("The configuration plan no longer matches its recipe.");
                    if (!Same(Bytes(before), Bytes(edit.Bytes))) await WriteAsync(scope, current, edit.Bytes, pending, cancellationToken).ConfigureAwait(false);
                    addition = edit.Addition;
                }
                var index = remaining.FindIndex(receipt => receipt.Id == observation.Change.Id);
                if (addition is not null)
                {
                    if (index >= 0) remaining[index] = addition;
                    else remaining.Add(addition);
                }
                if (observation.Change is ToolMarkedBlockChange { WhenNotIgnored: { } coverage }
                    && !await new ToolEffectiveIgnore(_processes).IsIgnoredAsync(plan.Request.Context, coverage.WorkspaceRelativePath, cancellationToken).ConfigureAwait(false))
                    throw new InvalidDataException("The conditional marked block did not verify effective ignore coverage.");
                completed.Add(observation.Change.Id);
            }
        }
        catch (Exception exception) when (ExpectedFailure(exception) || exception is OperationCanceledException) { failure = exception.Message; }
        return new()
        {
            RemainingAdditions = remaining,
            CompletedChanges = completed,
            RemovedChanges = removed,
            PendingBackups = pending.Values.ToArray(),
            NativeSettings = nativeSettings,
            Unfinished = plan.Removals.Where(change => !removed.Contains(change.Addition.Id, StringComparer.Ordinal)).Select(change => change.Addition.Id)
                .Concat(plan.Changes.Where(change => !completed.Contains(change.Change.Id, StringComparer.Ordinal)).Select(change => change.Change.Id)).ToArray(),
            Failure = failure,
        };
    }

    private async Task WriteAsync(CliWorkspace scope, FileStateSnapshot current, byte[]? bytes, IDictionary<string, ToolBackupCompletionRequest> pending, CancellationToken token)
    {
        var backup = await _backups.PrepareAsync(new(current.LogicalPath), token).ConfigureAwait(false);
        if (backup.BackupPath is not null) pending[current.LogicalPath] = new(backup, ExpectedAppliedSha256: null, Verified: false);
        if (backup.Failure is not null) throw new IOException(backup.Failure);
        pending[current.LogicalPath] = new(backup, Hash(bytes), Verified: false);
        await ToolFileReplacement.WriteAsync(scope, current.Expectation, bytes, token).ConfigureAwait(false);
        var applied = await ToolFileReader.ReadAsync(scope, current.LogicalPath, token).ConfigureAwait(false);
        if (applied.ContentHash != Hash(bytes)) throw new IOException("The file effect did not verify. Keep its backup for manual checking.");
        pending[current.LogicalPath] = new(backup, applied.ContentHash, Verified: true);
    }

    private static ToolNativeSettingRecord? NativeFact(ToolConfigRequest request, ToolNativeConfigChange change)
    {
        var home = ToolNativePaths.HomeIdentity(request.Context.NativeHomePath);
        return request.NativeSettings.SingleOrDefault(fact => fact.ToolId == request.ToolId && fact.ChangeId == change.Id && !string.IsNullOrWhiteSpace(fact.NativeHomePath)
            && ToolNativePaths.Comparer.Equals(ToolNativePaths.HomeIdentity(fact.NativeHomePath), home));
    }

    private static string[] ManualArguments(ToolRecipeContext context, ToolNativeConfigChange change, string previous)
    {
        if (previous is not (ToolNativeConfigDefinitions.EnabledValue or ToolNativeConfigDefinitions.DisabledValue)) throw new InvalidDataException("The previous native setting is unknown.");
        var arguments = change.ManualRestoreArgumentsTemplate.Select(value => ToolRecipeExpansion.Expand(value, context, previous)).ToArray();
        if (!arguments.SequenceEqual(new[] { ToolNativeSettings.ApplyArguments[0], previous }, StringComparer.Ordinal))
            throw new InvalidDataException("The manual native setting command is outside its allow-list.");
        return arguments;
    }
}
