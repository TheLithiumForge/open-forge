using System.Collections.Immutable;
using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;

internal enum ExtensionInstallFindingCode
{
    InvalidInput,
    SelectionRequired,
    InteractionEnded,
    ConfirmationRequired,
    SourceUnavailable,
    SourceInvalid,
    FrameworkUnavailable,
    FrameworkUnsafe,
    LifecycleUnavailable,
    LifecycleBlocked,
    ManagedDivergence,
    PackageContentsChanged,
    InitialForceRequired,
    OwnershipConflict,
    PermissionRequired,
    PermissionDeclined,
    PermissionsInvalid,
    PermissionsUnavailable,
    PermissionsChanged,
    PermissionWriteFailed,
    TargetUnsafe,
    ProjectionUnavailable,
    GeneratedRegionUnsafe,
    WorkspaceLockUnavailable,
    TargetChanged,
    RecoveryConflict,
    RecoveryUnavailable,
    LifecycleObservation,
    PackageContentMissing,
    RecoveryArtifactRetained,
    WriteFailed,
    TopologyVerificationFailed,
    LifecyclePublicationFailed,
    VerificationFailed,
    RecoveryFailed,
    OperationFailed,
    Interrupted,
    MetadataProjectionSkipped,
}

internal enum ExtensionInstallSelectionKind
{
    ExplicitIds,
    ExplicitAll,
    SinglePackageInference,
    InteractiveIds,
    InteractiveAll,
}

internal enum ExtensionInstallSourceKind
{
    Embedded,
    Package,
    Catalogue,
}

internal enum ExtensionInstallEffectKind
{
    Directory,
    PackageFile,
    GeneratedRegion,
}

internal enum ExtensionInstallEffectAction
{
    Create,
    Replace,
}

internal enum ExtensionInstallEffectOutcome
{
    Planned,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum ExtensionInstallEffectResidual
{
    None,
    Retained,
    Unknown,
}

internal enum ExtensionInstallGeneratedRegionState
{
    Changed,
    Unchanged,
}

internal enum ExtensionInstallLifecycleAction
{
    None,
    Preserve,
    Publish,
}

internal enum ExtensionInstallLifecycleOutcome
{
    NotRequested,
    Planned,
    AlreadyCurrent,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum ExtensionInstallRecoveryState
{
    NotRequired,
    NotCreated,
    Removed,
    Retained,
    Unknown,
}

internal enum ExtensionInstallVerificationState
{
    NotRequested,
    Planned,
    Verified,
    Failed,
    Unknown,
}

internal sealed record ExtensionInstallSelection
{
    internal ExtensionInstallSelection(
        ExtensionInstallSelectionKind selectedBy,
        IEnumerable<string> rootIds)
    {
        SelectedBy = selectedBy;
        RootIds = ExtensionInstallResultSnapshots.SnapshotStrings(rootIds, nameof(rootIds));
    }

    internal ExtensionInstallSelectionKind SelectedBy { get; }

    internal IReadOnlyList<string> RootIds { get; }
}

internal sealed record ExtensionInstallSource(
    ExtensionInstallSourceKind Kind,
    string? Path,
    string Identity,
    int PackageCount);

internal sealed record ExtensionInstallPackage
{
    internal ExtensionInstallPackage(
        string id,
        string version,
        bool selectedRoot,
        IEnumerable<string> dependencies)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        Id = id;
        Version = version;
        SelectedRoot = selectedRoot;
        Dependencies = ExtensionInstallResultSnapshots.SnapshotStrings(dependencies, nameof(dependencies));
    }

    internal string Id { get; }

    internal string Version { get; }

    internal bool SelectedRoot { get; }

    internal IReadOnlyList<string> Dependencies { get; }
}

internal sealed record ExtensionInstallFramework(
    string InventoryFingerprint,
    int TargetCount,
    int GeneratedRegionCount);

internal sealed record ExtensionInstallFootprint
{
    internal ExtensionInstallFootprint(
        int packageCount,
        IEnumerable<string> payloadTargets,
        IEnumerable<string> generatedRegions,
        IEnumerable<string> directories)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(packageCount);

        PackageCount = packageCount;
        PayloadTargets = ExtensionInstallResultSnapshots.SnapshotStrings(payloadTargets, nameof(payloadTargets));
        GeneratedRegions = ExtensionInstallResultSnapshots.SnapshotStrings(generatedRegions, nameof(generatedRegions));
        Directories = ExtensionInstallResultSnapshots.SnapshotStrings(directories, nameof(directories));
    }

    internal int PackageCount { get; }

    internal IReadOnlyList<string> PayloadTargets { get; }

    internal IReadOnlyList<string> GeneratedRegions { get; }

    internal IReadOnlyList<string> Directories { get; }
}

internal sealed record ExtensionInstallEffect(
    string Path,
    string? PackageId,
    ExtensionInstallEffectKind Kind,
    ExtensionInstallEffectAction Action,
    ExtensionInstallEffectOutcome Outcome,
    ExtensionInstallEffectResidual Residual);

internal sealed record ExtensionInstallGeneratedRegion(
    string Path,
    ExtensionInstallGeneratedRegionState State);

internal sealed record ExtensionInstallGeneratedNavigation
{
    internal ExtensionInstallGeneratedNavigation(
        IEnumerable<ExtensionInstallGeneratedRegion> regions)
    {
        ArgumentNullException.ThrowIfNull(regions);
        Regions = new ReadOnlyCollection<ExtensionInstallGeneratedRegion>([.. regions
            .Select(value => value ?? throw new ArgumentException(
                "Generated Navigation regions cannot contain null members.",
                nameof(regions)))]);
    }

    internal IReadOnlyList<ExtensionInstallGeneratedRegion> Regions { get; }
}

internal sealed record ExtensionInstallLifecycle(
    ExtensionInstallLifecycleAction Action,
    ExtensionInstallLifecycleOutcome Outcome);

internal sealed record ExtensionInstallRecovery
{
    internal ExtensionInstallRecovery(
        ExtensionInstallRecoveryState state,
        IEnumerable<string> protectedPaths,
        string? residualPath)
    {
        State = state;
        ProtectedPaths = ExtensionInstallResultSnapshots.SnapshotStrings(protectedPaths, nameof(protectedPaths));
        ResidualPath = residualPath;
    }

    internal ExtensionInstallRecoveryState State { get; }

    internal IReadOnlyList<string> ProtectedPaths { get; }

    internal string? ResidualPath { get; }
}

internal sealed record ExtensionInstallVerification(
    ExtensionInstallVerificationState Targets,
    ExtensionInstallVerificationState Topology,
    ExtensionInstallVerificationState ExtensionsLifecycle,
    ExtensionInstallVerificationState FrameworkLifecycle);

internal sealed record ExtensionInstallFinding
{
    internal ExtensionInstallFinding(
        ExtensionInstallFindingCode code,
        string cause,
        string? target = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        Code = code;
        Status = ExtensionInstallDefinitions.ReadStatus(code);
        Target = target;
        Cause = cause;
    }

    internal ExtensionInstallFindingCode Code { get; }

    internal CliSemanticStatus Status { get; }

    internal string? Target { get; }

    internal string Cause { get; }
}

internal sealed record ExtensionInstallResultFacts
{
    internal required ExtensionInstallSelection? Selection { get; init; }

    internal required ExtensionInstallSource? Source { get; init; }

    internal required IReadOnlyList<ExtensionInstallPackage> Packages { get; init; }

    internal required ExtensionInstallFramework? Framework { get; init; }

    internal required ExtensionInstallFootprint? Footprint { get; init; }

    internal required IReadOnlyList<ExtensionInstallEffect> Effects { get; init; }

    internal required ExtensionInstallGeneratedNavigation? GeneratedNavigation { get; init; }

    internal WorkspacePermissionResult Permissions { get; init; } = WorkspacePermissionResult.NotEvaluated;

    internal required ExtensionInstallLifecycle Lifecycle { get; init; }

    internal required ExtensionInstallRecovery Recovery { get; init; }

    internal required ExtensionInstallVerification Verification { get; init; }

    internal static ExtensionInstallResultFacts Snapshot(ExtensionInstallResultFacts value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new ExtensionInstallResultFacts
        {
            Selection = value.Selection is null
                ? null
                : new ExtensionInstallSelection(
                    value.Selection.SelectedBy,
                    value.Selection.RootIds),
            Source = value.Source,
            Packages = [.. value.Packages.Select(package => new ExtensionInstallPackage(
                package.Id,
                package.Version,
                package.SelectedRoot,
                package.Dependencies))],
            Framework = value.Framework,
            Footprint = value.Footprint is null
                ? null
                : new ExtensionInstallFootprint(
                    value.Footprint.PackageCount,
                    value.Footprint.PayloadTargets,
                    value.Footprint.GeneratedRegions,
                    value.Footprint.Directories),
            Effects = [.. value.Effects],
            GeneratedNavigation = value.GeneratedNavigation is null
                ? null
                : new ExtensionInstallGeneratedNavigation(
                    value.GeneratedNavigation.Regions),
            Permissions = value.Permissions,
            Lifecycle = value.Lifecycle,
            Recovery = new ExtensionInstallRecovery(
                value.Recovery.State,
                value.Recovery.ProtectedPaths,
                value.Recovery.ResidualPath),
            Verification = value.Verification,
        };
    }
}

internal sealed record ExtensionInstallResult : ICliCommandResult
{
    internal ExtensionInstallResult(
        ExtensionInstallRequest request,
        ExtensionInstallResultFacts facts,
        IEnumerable<ExtensionInstallFinding> findings)
        : this(
            ExtensionInstallResultInput.FromRequest(request),
            facts,
            findings)
    {
    }

    private ExtensionInstallResult(
        ExtensionInstallResultInput input,
        ExtensionInstallResultFacts facts,
        IEnumerable<ExtensionInstallFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(facts);
        ArgumentNullException.ThrowIfNull(findings);

        Workspace = input.Workspace;
        Mode = input.Mode;
        Force = input.Force;
        Automatic = input.Automatic;
        Selection = facts.Selection;
        Source = facts.Source;
        Packages = Snapshot(facts.Packages, nameof(facts.Packages));
        Framework = facts.Framework;
        Footprint = facts.Footprint;
        Effects = Snapshot(facts.Effects, nameof(facts.Effects));
        GeneratedNavigation = facts.GeneratedNavigation;
        Permissions = facts.Permissions;
        Lifecycle = facts.Lifecycle;
        Recovery = facts.Recovery;
        Verification = facts.Verification;
        Findings = new ReadOnlyCollection<ExtensionInstallFinding>([.. findings
            .Select(value => value ?? throw new ArgumentException(
                "Extension Install findings cannot contain null members.",
                nameof(findings)))
            .OrderBy(value => value.Code)
            .ThenBy(value => value.Target is null ? 0 : 1)
            .ThenBy(value => value.Target, StringComparer.Ordinal)
            .ThenBy(value => value.Cause, StringComparer.Ordinal)]);
        Status = ReadStatus(Findings);
        Next = ExtensionInstallDefinitions.ReadNextAction(Status, Findings, input.Request);
    }

    public string Command => ExtensionInstallDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    internal string? WorkspacePath => Workspace?.LexicalRoot;

    internal bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;

    public CliNextAction? Next { get; }

    internal ExtensionInstallMode Mode { get; }

    internal bool Force { get; }

    internal bool Automatic { get; }

    internal ExtensionInstallSelection? Selection { get; }

    internal ExtensionInstallSource? Source { get; }

    internal IReadOnlyList<ExtensionInstallPackage> Packages { get; }

    internal ExtensionInstallFramework? Framework { get; }

    internal ExtensionInstallFootprint? Footprint { get; }

    internal IReadOnlyList<ExtensionInstallEffect> Effects { get; }

    internal ExtensionInstallGeneratedNavigation? GeneratedNavigation { get; }

    internal WorkspacePermissionResult Permissions { get; }

    internal ImmutableArray<string> RequiredPermissions => Permissions.Required;

    internal ImmutableArray<string> MissingPermissions => Permissions.Missing;

    internal Enum PermissionDecision => Permissions.Decision;

    internal Enum PermissionOutcome => Permissions.Outcome;

    internal ExtensionInstallLifecycle Lifecycle { get; }

    internal ExtensionInstallRecovery Recovery { get; }

    internal ExtensionInstallVerification Verification { get; }

    internal IReadOnlyList<ExtensionInstallFinding> Findings { get; }

    internal static ExtensionInstallResult Empty(
        CliWorkspace? workspace,
        ExtensionInstallMode mode,
        bool force,
        bool automatic,
        params ExtensionInstallFinding[] findings)
        => new(
            ExtensionInstallResultInput.FromBinding(
                workspace,
                mode,
                force,
                automatic),
            new ExtensionInstallResultFacts
            {
                Selection = null,
                Source = null,
                Packages = [],
                Framework = null,
                Footprint = null,
                Effects = [],
                GeneratedNavigation = null,
                Lifecycle = new ExtensionInstallLifecycle(
                    ExtensionInstallLifecycleAction.None,
                    ExtensionInstallLifecycleOutcome.NotRequested),
                Recovery = new ExtensionInstallRecovery(
                    ExtensionInstallRecoveryState.NotRequired,
                    [],
                    residualPath: null),
                Verification = new ExtensionInstallVerification(
                    ExtensionInstallVerificationState.NotRequested,
                    ExtensionInstallVerificationState.NotRequested,
                    ExtensionInstallVerificationState.NotRequested,
                    ExtensionInstallVerificationState.NotRequested),
            },
            findings);

    private sealed record ExtensionInstallResultInput(
        CliWorkspace? Workspace,
        ExtensionInstallMode Mode,
        bool Force,
        bool Automatic,
        ExtensionInstallRequest? Request)
    {
        internal static ExtensionInstallResultInput FromRequest(ExtensionInstallRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            return new(
                request.Workspace,
                request.Mode,
                request.Force,
                request.Automatic,
                request);
        }

        internal static ExtensionInstallResultInput FromBinding(
            CliWorkspace? workspace,
            ExtensionInstallMode mode,
            bool force,
            bool automatic)
            => new(workspace, mode, force, automatic, Request: null);
    }

    private static ReadOnlyCollection<T> Snapshot<T>(
        IEnumerable<T> values,
        string parameterName)
        where T : class
        => new([.. values
            .Select(value => value ?? throw new ArgumentException(
                "Extension Install result collections cannot contain null members.",
                parameterName))]);

    private static CliSemanticStatus ReadStatus(
        IReadOnlyList<ExtensionInstallFinding> findings)
        => CliStatusDefinitions.Collapse(findings.Select(finding => finding.Status));
}

file static class ExtensionInstallResultSnapshots
{
    internal static IReadOnlyList<string> SnapshotStrings(
        IEnumerable<string> values,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        return new ReadOnlyCollection<string>([.. values
            .Select(value => value ?? throw new ArgumentException(
                "Extension Install string collections cannot contain null members.",
                parameterName))]);
    }
}
