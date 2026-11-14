using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Shared.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Observation;

// Shared read facts for Tool Inspect, Tool Status, workspace Status and Doctor.
// Contract: .agents/memory/crystallized/documents/cli/contracts/tool/behavior.md
internal sealed class ToolObservationReader(IToolChannelService channels, IToolConfigService config, IToolVersionService versions, IToolPathHost paths,
    Func<ToolObservationRequest, ToolVersionProfile, ToolDetectionResult, ToolRecipeContext> recipeContext)
{
    internal async Task<ToolObservation> ReadAsync(ToolObservationRequest request, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var id = request.Entry.Identity.Id;
        var portable = request.Records.Portable;
        var setting = portable?.Settings.GetValueOrDefault(id);
        var locked = portable?.Lock.GetValueOrDefault(id);
        var machine = request.Records.Machine.GetValueOrDefault(id);
        var detection = await channels.DetectAsync(new ToolDetectionRequest(request.Entry, request.Locations.WorkspacePath) { RecordedState = machine }, token).ConfigureAwait(false);
        var managers = await channels.DetectManagersAsync(request.Locations.WorkspacePath, token).ConfigureAwait(false);
        var path = await paths.ObserveAsync(request.Locations.HomePath, token).ConfigureAwait(false);
        var profile = request.Entry.Profiles.FirstOrDefault(candidate => detection.Version is { } version && Supports(candidate, version));
        var issues = new List<ToolObservationIssue>();
        ToolObservationState state;
        if (detection.Failure is not null || detection.CommandPath is not null && detection.Version is null)
        { state = ToolObservationState.Unavailable; issues.Add(new(ToolObservationIssueKind.Unavailable)); }
        else if (detection.CommandPath is null)
        { state = ToolObservationState.Missing; issues.Add(new(ToolObservationIssueKind.Missing)); }
        else if (profile is null)
        { state = ToolObservationState.Unsupported; issues.Add(new(ToolObservationIssueKind.Unsupported)); }
        else state = ToolObservationState.Current;
        if (machine is null) issues.Add(new(ToolObservationIssueKind.MachineRecordMissing));
        if (path.Readiness != ToolPathReadiness.Ready) issues.Add(new(ToolObservationIssueKind.PathNotReady));
        if (locked is null && setting is not null) issues.Add(new(ToolObservationIssueKind.LockMissing));
        if (detection.Version is not null && (locked is not null && locked.Version != detection.Version || machine is not null && machine.Version != detection.Version))
            issues.Add(new(ToolObservationIssueKind.VersionChanged));
        ToolConfigPlan? configuration = null;
        var native = request.Records.WorkspaceMachine?.NativeSettings.Where(value => value.ToolId == id).ToArray() ?? [];
        if (profile is not null && detection.Invocation is not null)
        {
            var context = recipeContext(request, profile, detection);
            var nativeHome = ToolRecordPaths.ResolveDirectory(context.NativeHomePath);
            context = context with { NativeHomePath = nativeHome };
            var boundNative = native.Where(value => PhysicalIdentityTracker.PathComparer.Equals(value.NativeHomePath, nativeHome)).ToArray();
            var digest = config.ComputeProfileDigest(profile);
            if (locked?.ProfileDigest is { } applied && applied != digest) issues.Add(new(ToolObservationIssueKind.ProfileChanged));
            foreach (var change in locked?.Changes.OfType<ToolNativeCommandAddition>() ?? [])
                if (!boundNative.Any(value => value.ChangeId == change.Id)) issues.Add(new(ToolObservationIssueKind.NativeRecordMissing, change.Id));
            configuration = await config.PlanAsync(new ToolConfigRequest
            {
                ToolId = id,
                Context = context,
                ProfileDigest = digest,
                Invocation = detection.Invocation,
                InstalledInvocation = detection.Invocation,
                Changes = profile.Changes,
                AppliedChanges = locked?.Changes ?? [],
                NativeSettings = native,
            }, token).ConfigureAwait(false);
            foreach (var change in configuration.Changes.Where(value => value.Disposition != ToolChangeDisposition.NoChange))
                issues.Add(new(ToolObservationIssueKind.ConfigurationDrift, change.Change.Id));
            foreach (var removal in configuration.Removals.Where(value => value.Disposition != ToolChangeDisposition.NoChange))
                issues.Add(new(ToolObservationIssueKind.ConfigurationDrift, removal.Addition.Id));
        }
        if (state == ToolObservationState.Current && issues.Count > 0) state = ToolObservationState.Changed;
        token.ThrowIfCancellationRequested();
        return new ToolObservation
        {
            Entry = request.Entry,
            Setting = setting,
            Locked = locked,
            Machine = machine,
            MachineRecordState = RecordState(request.Records, machine),
            NativeRecordState = request.Records.WorkspaceMachineState,
            NativeSettings = native,
            Detection = detection,
            Profile = profile,
            Configuration = configuration,
            Managers = managers,
            Path = path,
            State = state,
            Issues = issues.ToArray(),
        };
    }

    private static ToolRecordReadState RecordState(ToolRecordsResult records, ToolMachineState? machine)
    {
        if (machine is not null) return ToolRecordReadState.Available;
        if (records.MachineState == ToolRecordReadState.Available) return ToolRecordReadState.Missing;
        return records.MachineState;
    }

    private bool Supports(ToolVersionProfile profile, string version)
        => versions.Satisfies(version, new ToolVersionSelection { Kind = ToolVersionSelectionKind.MinorRange, Range = profile.Range })
            && Version.TryParse(version.Split('+')[0], out var found) && Version.TryParse(profile.MinimumToolVersion.Split('+')[0], out var minimum) && found >= minimum;
}
