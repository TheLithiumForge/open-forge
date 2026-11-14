using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges.Models;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Tools.Shared.NativeSettings;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;

internal sealed partial class ToolConfigService : IToolConfigService, IToolSetupService
{
    private readonly IToolProcessHost _processes;
    private readonly ToolBackupService _backups;
    private readonly Func<string, byte[]> _readTemplate;
    private readonly ToolNativeSettings _native;
    private readonly ToolSetupPlanner _setups;

    internal ToolConfigService(IToolProcessHost processes, ToolBackupService backups, IToolNativePlanReader nativePlans)
        : this(processes, backups, nativePlans, ToolProfileDigest.Template) { }

    internal ToolConfigService(IToolProcessHost processes, ToolBackupService backups, IToolNativePlanReader nativePlans, Func<string, byte[]> readTemplate)
    {
        _processes = processes;
        _backups = backups;
        _readTemplate = readTemplate;
        _native = new(processes);
        _setups = new(processes, nativePlans, backups);
    }

    public string ComputeProfileDigest(ToolVersionProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return ToolProfileDigest.Compute(profile, _readTemplate);
    }

    public Task<ToolSetupPlan> PlanAsync(ToolSetupRequest request, CancellationToken cancellationToken) => _setups.PlanAsync(request, cancellationToken);

    public async Task<ToolConfigPlan> PlanAsync(ToolConfigRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ToolRecipeExpansion.Validate(request.Context);
        ValidateIds(request);
        var observations = new List<ToolConfigObservation>();
        var removals = new List<ToolConfigRemovalObservation>();
        var simulated = new Dictionary<string, byte[]?>(ToolNativePaths.Comparer);
        var remaining = request.AppliedChanges.ToList();
        foreach (var prior in request.AppliedChanges)
        {
            var recipe = request.Changes.SingleOrDefault(change => change.Id == prior.Id);
            if (recipe is not null)
            {
                try { if (SameTarget(ToolRecipeExpansion.Change(recipe, request.Context), prior)) continue; }
                catch (Exception exception) when (ExpectedFailure(exception)) { continue; }
            }
            var observation = await ObserveRemovalAsync(request.Context, prior, simulated, cancellationToken).ConfigureAwait(false);
            if (recipe is not null && observation.Disposition != ToolChangeDisposition.Conflict)
            {
                try { RequireMigrationMatch(prior, observation.Current); }
                catch (Exception exception) when (ExpectedFailure(exception))
                {
                    observation = observation with { Disposition = ToolChangeDisposition.Conflict, Desired = observation.Current, Conflict = exception.Message };
                    if (prior is not ToolNativeCommandAddition)
                    {
                        var target = ToolFileReader.Target(ToolFileReader.Scope(request.Context.WorkspacePath), ToolConfigEditor.PathOf(prior));
                        simulated[target] = observation.Current.Present ? observation.Current.Bytes : null;
                    }
                }
            }
            removals.Add(observation);
            if (observation.Disposition != ToolChangeDisposition.Conflict) remaining.Remove(prior);
        }
        foreach (var change in request.Changes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = Bytes(null);
            IReadOnlyDictionary<string, string>? nativeEnvironmentOverrides = null;
            try
            {
                if (change is ToolNativeConfigChange native)
                {
                    nativeEnvironmentOverrides = ToolNativeSettings.EnvironmentOverrides(native, request.Context.NativeEnvironment);
                    current = await _native.ReadAsync(request, native, cancellationToken).ConfigureAwait(false);
                    var fact = NativeFact(request, native);
                    if (fact is not null && !SameNative(current, fact.Applied)) throw new InvalidDataException("The recorded native setting has drifted.");
                    _ = ManualArguments(request.Context, native, fact?.Previous.Native ?? current.Native ?? throw new InvalidDataException("The native previous value is unavailable."));
                    var recorded = remaining.OfType<ToolNativeCommandAddition>().Any(receipt => receipt.Id == native.Id && receipt.AllowListId == native.AllowListId);
                    observations.Add(new()
                    {
                        Change = change,
                        Current = current,
                        Desired = ToolNativeSettings.Value(native.DesiredValue),
                        NativeEnvironmentOverrides = nativeEnvironmentOverrides,
                        Disposition = recorded && fact is not null && current.Native == native.DesiredValue ? ToolChangeDisposition.NoChange : ToolChangeDisposition.Add
                    });
                    continue;
                }
                var resolved = ToolRecipeExpansion.Change(change, request.Context);
                if (resolved.Scope != ToolEffectScope.Workspace) throw new InvalidDataException("A file addition must have workspace scope.");
                var target = ToolFileReader.Target(ToolFileReader.Scope(request.Context.WorkspacePath), ToolConfigEditor.PathOf(resolved));
                var before = await ReadSimulatedAsync(request.Context, target, simulated, cancellationToken).ConfigureAwait(false);
                current = Bytes(before);
                var edit = await EditAsync(request with { AppliedChanges = remaining }, resolved, before, cancellationToken).ConfigureAwait(false);
                if (!Same(current, Bytes(edit.Bytes))) await _backups.CheckFeasibilityAsync(target, cancellationToken).ConfigureAwait(false);
                observations.Add(new() { Change = change, Current = current, Desired = Bytes(edit.Bytes), Disposition = edit.Disposition });
                simulated[target] = edit.Bytes;
            }
            catch (Exception exception) when (ExpectedFailure(exception))
            {
                observations.Add(new()
                {
                    Change = change,
                    Current = current,
                    Desired = current,
                    Disposition = ToolChangeDisposition.Conflict,
                    Conflict = exception.Message,
                    NativeEnvironmentOverrides = nativeEnvironmentOverrides
                });
            }
        }
        return new(request, observations, removals);
    }

    private async Task<ToolConfigEdit> EditAsync(ToolConfigRequest request, ToolConfigChange resolved, byte[]? before, CancellationToken token)
    {
        var prior = request.AppliedChanges.SingleOrDefault(addition => addition.Id == resolved.Id);
        if (resolved is ToolMarkedBlockChange { WhenNotIgnored: { } condition }
            && await new ToolEffectiveIgnore(_processes).IsIgnoredAsync(request.Context, condition.WorkspaceRelativePath, token).ConfigureAwait(false))
        {
            if (prior is ToolMarkedBlockAddition receipt)
            {
                var span = ToolMarkedBlocks.Find(before ?? [], receipt.StartMarker, receipt.EndMarker);
                if (span is null || before is null || FileExpectation.Hash(before.AsSpan(span.Start, span.End - span.Start)) != receipt.ContentSha256)
                    throw new InvalidDataException("The recorded conditional block has drifted.");
            }
            return new(before, prior, ToolChangeDisposition.NoChange);
        }
        var edit = resolved switch
        {
            ToolJsonValueChange json => ToolConfigEditor.Json(json, before, prior),
            ToolMarkedBlockChange block => ToolConfigEditor.Block(block, before, prior),
            ToolManagedFileChange file => ToolConfigEditor.File(file, before, ToolConfigBytes.Utf8.GetBytes(ToolRecipeExpansion.Expand(ToolConfigBytes.Utf8.GetString(_readTemplate(file.TemplateResource)), request.Context)), prior),
            _ => throw new InvalidDataException("The config recipe kind is unsupported."),
        };
        if (resolved is ToolMarkedBlockChange { WhenNotIgnored: not null } conditional)
            await new ToolEffectiveIgnore(_processes).RequireCoverageAsync(request.Context, conditional, edit.Bytes, token).ConfigureAwait(false);
        return edit;
    }

    private static bool SameTarget(ToolConfigChange change, ToolAppliedChange prior) => (change, prior) switch
    {
        (ToolJsonValueChange json, ToolJsonUnionAddition union) => json.Operation == ToolJsonValueOperation.Union && json.PathTemplate == union.Path && json.Pointer == union.Pointer,
        (ToolJsonValueChange json, ToolJsonValueAddition value) => json.Operation == ToolJsonValueOperation.SetIfAbsent && json.PathTemplate == value.Path && json.Pointer == value.Pointer,
        (ToolMarkedBlockChange block, ToolMarkedBlockAddition receipt) => block.PathTemplate == receipt.Path,
        (ToolManagedFileChange file, ToolManagedFileAddition receipt) => file.PathTemplate == receipt.Path,
        (ToolNativeConfigChange native, ToolNativeCommandAddition receipt) => native.AllowListId == receipt.AllowListId,
        _ => false,
    };

    private static void RequireMigrationMatch(ToolAppliedChange addition, ToolRecordedValue current)
    {
        if (addition is ToolJsonUnionAddition or ToolNativeCommandAddition) return;
        if (!current.Present || current.Bytes is not { } bytes) throw new InvalidDataException("The recorded addition is missing and cannot migrate.");
        switch (addition)
        {
            case ToolJsonValueAddition value:
                using (var entries = new ToolJsonEntries(bytes))
                    if (entries.Value(value.Pointer) is not { } found || !System.Text.Json.JsonElement.DeepEquals(found, value.Value))
                        throw new InvalidDataException("The recorded JSON value has drifted and cannot migrate.");
                break;
            case ToolManagedFileAddition file:
                if (FileExpectation.Hash(bytes) != file.Sha256) throw new InvalidDataException("The recorded managed file has drifted and cannot migrate.");
                break;
            case ToolMarkedBlockAddition block:
                var span = ToolMarkedBlocks.Find(bytes, block.StartMarker, block.EndMarker);
                if (span is null || FileExpectation.Hash(bytes.AsSpan(span.Start, span.End - span.Start)) != block.ContentSha256)
                    throw new InvalidDataException("The recorded marked block has drifted and cannot migrate.");
                break;
            default: throw new ArgumentOutOfRangeException(nameof(addition));
        }
    }

    private static void ValidateIds(ToolConfigRequest request)
    {
        if (request.Changes.Any(change => string.IsNullOrWhiteSpace(change.Id)) || request.Changes.Select(change => change.Id).Distinct(StringComparer.Ordinal).Count() != request.Changes.Count
            || request.AppliedChanges.Select(change => change.Id).Distinct(StringComparer.Ordinal).Count() != request.AppliedChanges.Count)
            throw new InvalidDataException("Configuration change IDs must be nonempty and unique.");
    }

    private static ToolRecordedValue Bytes(byte[]? bytes) => new() { Present = bytes is not null, Kind = ToolRecordedValueKind.Bytes, Bytes = bytes };
    private static string? Hash(byte[]? bytes) => bytes is null ? null : FileExpectation.Hash(bytes);
    private static bool SameNative(ToolRecordedValue left, ToolRecordedValue right) => left.Kind == ToolRecordedValueKind.Native && right.Kind == ToolRecordedValueKind.Native && left.Native == right.Native;
    private static bool Same(ToolRecordedValue left, ToolRecordedValue right)
        => left.Kind == ToolRecordedValueKind.Bytes && right.Kind == ToolRecordedValueKind.Bytes && left.Present == right.Present
            && (!left.Present || left.Bytes is not null && right.Bytes is not null && left.Bytes.AsSpan().SequenceEqual(right.Bytes));
    private static bool ExpectedFailure(Exception exception) => exception is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException or System.Text.Json.JsonException;
}
