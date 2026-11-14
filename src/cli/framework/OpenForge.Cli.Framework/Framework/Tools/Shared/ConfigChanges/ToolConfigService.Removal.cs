using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Tools.Shared.NativeSettings;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;

internal sealed partial class ToolConfigService
{
    public async Task<ToolConfigRemovePlan> PlanRemoveAsync(ToolConfigRemoveRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ToolRecipeExpansion.Validate(request.Context);
        if (request.Lock.Changes.Select(change => change.Id).Distinct(StringComparer.Ordinal).Count() != request.Lock.Changes.Count)
            throw new InvalidDataException("Removal receipts have duplicate change IDs.");
        var simulated = new Dictionary<string, byte[]?>(ToolNativePaths.Comparer);
        var observations = new List<ToolConfigRemovalObservation>();
        foreach (var addition in request.Lock.Changes)
            observations.Add(await ObserveRemovalAsync(request.Context, addition, simulated, cancellationToken).ConfigureAwait(false));
        return new(request, observations);
    }

    public async Task<ToolConfigRemoveResult> RemoveAsync(ToolConfigRemovePlan plan, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var removed = new List<string>();
        var remaining = plan.Request.Lock.Changes.ToList();
        var pending = new Dictionary<string, ToolBackupCompletionRequest>(ToolNativePaths.Comparer);
        string? failure = null;
        try
        {
            ToolRecipeExpansion.Validate(plan.Request.Context);
            ValidateRemovalSelection(plan.Changes, plan.Request.Lock.Changes);
            if (plan.Changes.Any(change => change.Disposition == ToolChangeDisposition.Conflict))
                throw new InvalidDataException(plan.Changes.First(change => change.Disposition == ToolChangeDisposition.Conflict).Conflict ?? "The removal plan conflicts.");
            foreach (var observation in plan.Changes)
            {
                await ApplyRemovalAsync(plan.Request.Context, observation, pending, cancellationToken).ConfigureAwait(false);
                remaining.RemoveAll(receipt => receipt.Id == observation.Addition.Id);
                removed.Add(observation.Addition.Id);
            }
        }
        catch (Exception exception) when (ExpectedFailure(exception) || exception is OperationCanceledException) { failure = exception.Message; }
        return new()
        {
            RemovedChanges = removed,
            RemainingAdditions = remaining,
            PendingBackups = pending.Values.ToArray(),
            Unfinished = plan.Changes.Where(change => !removed.Contains(change.Addition.Id, StringComparer.Ordinal)).Select(change => change.Addition.Id).ToArray(),
            KeptNativeSettings = plan.Request.NativeSettings.Where(setting => setting.ToolId == plan.Request.ToolId).ToArray(),
            Failure = failure,
        };
    }

    private static async Task<byte[]?> ReadSimulatedAsync(ToolRecipeContext context, string path, IDictionary<string, byte[]?> simulated, CancellationToken token)
    {
        if (simulated.TryGetValue(path, out var bytes)) return bytes;
        var snapshot = await ToolFileReader.ReadAsync(ToolFileReader.Scope(context.WorkspacePath), path, token).ConfigureAwait(false);
        return snapshot.HasBytes ? snapshot.Bytes.ToArray() : null;
    }

    private static void ValidateRemovalSelection(IReadOnlyList<ToolConfigRemovalObservation> observations, IReadOnlyList<ToolAppliedChange> receipts)
    {
        if (receipts.Select(receipt => receipt.Id).Distinct(StringComparer.Ordinal).Count() != receipts.Count
            || observations.Select(observation => observation.Addition.Id).Distinct(StringComparer.Ordinal).Count() != observations.Count
            || observations.Any(observation => !receipts.Any(receipt => SameReceipt(receipt, observation.Addition))))
            throw new InvalidDataException("A removal observation is duplicated or absent from the current receipt set.");
    }

    private static bool SameReceipt(ToolAppliedChange left, ToolAppliedChange right)
    {
        if (left.Id != right.Id) return false;
        return (left, right) switch
        {
            (ToolJsonValueAddition value, ToolJsonValueAddition other) => value.Path == other.Path && value.Pointer == other.Pointer && JsonElement.DeepEquals(value.Value, other.Value),
            (ToolJsonUnionAddition union, ToolJsonUnionAddition other) => union.Path == other.Path && union.Pointer == other.Pointer && union.Items.Count == other.Items.Count
                && union.Items.Zip(other.Items, JsonElement.DeepEquals).All(equal => equal),
            (ToolManagedFileAddition file, ToolManagedFileAddition other) => file.Path == other.Path && file.Sha256 == other.Sha256,
            (ToolMarkedBlockAddition block, ToolMarkedBlockAddition other) => block.Path == other.Path && block.StartMarker == other.StartMarker && block.EndMarker == other.EndMarker
                && block.ContentSha256 == other.ContentSha256,
            (ToolNativeCommandAddition native, ToolNativeCommandAddition other) => native.AllowListId == other.AllowListId,
            _ => false,
        };
    }

    private async Task<ToolConfigRemovalObservation> ObserveRemovalAsync(ToolRecipeContext context, ToolAppliedChange addition, IDictionary<string, byte[]?> simulated, CancellationToken token)
    {
        var current = Bytes(null);
        try
        {
            if (addition is ToolNativeCommandAddition)
            {
                var kept = ToolNativeSettings.Value(string.Empty, present: false);
                return new() { Addition = addition, Current = kept, Desired = kept, Disposition = ToolChangeDisposition.Keep };
            }
            var scope = ToolFileReader.Scope(context.WorkspacePath);
            var path = ToolFileReader.Target(scope, ToolConfigEditor.PathOf(addition));
            var before = await ReadSimulatedAsync(context, path, simulated, token).ConfigureAwait(false);
            current = Bytes(before);
            var after = ToolConfigEditor.Remove(addition, before);
            if (!Same(current, Bytes(after))) await _backups.CheckFeasibilityAsync(path, token).ConfigureAwait(false);
            simulated[path] = after;
            return new() { Addition = addition, Current = current, Desired = Bytes(after), Disposition = ToolChangeDisposition.Remove };
        }
        catch (Exception exception) when (ExpectedFailure(exception))
        {
            return new() { Addition = addition, Current = current, Desired = current, Disposition = ToolChangeDisposition.Conflict, Conflict = exception.Message };
        }
    }

    private async Task ApplyRemovalAsync(ToolRecipeContext context, ToolConfigRemovalObservation observation, IDictionary<string, ToolBackupCompletionRequest> pending, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (observation.Addition is ToolNativeCommandAddition) return;
        var scope = ToolFileReader.Scope(context.WorkspacePath);
        var path = ToolFileReader.Target(scope, ToolConfigEditor.PathOf(observation.Addition));
        var current = await ToolFileReader.ReadAsync(scope, path, token).ConfigureAwait(false);
        var before = current.HasBytes ? current.Bytes.ToArray() : null;
        if (!Same(Bytes(before), observation.Current)) throw new IOException("The configuration file changed after removal planning.");
        var after = ToolConfigEditor.Remove(observation.Addition, before);
        if (!Same(Bytes(after), observation.Desired)) throw new IOException("The removal plan no longer matches its receipt.");
        if (!Same(Bytes(before), Bytes(after))) await WriteAsync(scope, current, after, pending, token).ConfigureAwait(false);
    }
}
