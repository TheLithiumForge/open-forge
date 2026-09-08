using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;

internal sealed record ExtensionUpdateSource
{
    internal ExtensionUpdateSource(
        ExtensionUpdateSourceKind kind,
        string? path,
        string identity,
        int packageCount)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The Extension Update source kind is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(identity);
        if (packageCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(packageCount),
                packageCount,
                "The Extension Update source package count cannot be negative.");
        }

        Kind = kind;
        Path = path;
        Identity = identity;
        PackageCount = packageCount;
    }

    internal ExtensionUpdateSourceKind Kind { get; }

    internal string? Path { get; }

    internal string Identity { get; }

    internal int PackageCount { get; }
}

internal sealed record ExtensionUpdatePackage
{
    internal ExtensionUpdatePackage(
        string id,
        bool selectedRoot,
        IEnumerable<string> dependencies)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(dependencies);
        var values = dependencies
            .Select(value => value ?? throw new ArgumentException(
                "Extension Update package dependencies cannot contain null members.",
                nameof(dependencies)))
            .ToArray();
        if (values.Any(string.IsNullOrWhiteSpace)
            || values.Distinct(StringComparer.Ordinal).Count() != values.Length)
        {
            throw new ArgumentException(
                "Extension Update package dependencies must be unique and non-empty.",
                nameof(dependencies));
        }

        Id = id;
        SelectedRoot = selectedRoot;
        Dependencies = new ReadOnlyCollection<string>(values);
    }

    internal string Id { get; }

    internal bool SelectedRoot { get; }

    internal IReadOnlyList<string> Dependencies { get; }
}

internal sealed record ExtensionUpdateComparison
{
    internal ExtensionUpdateComparison(
        string path,
        string packageId,
        ExtensionUpdateComparisonTargetKind kind,
        string? region,
        string? sourceAssetPath,
        ExtensionUpdateComparisonFingerprintKind fingerprintKind,
        string? baselineFingerprint,
        string? currentFingerprint,
        string? intendedFingerprint,
        ExtensionUpdateComparisonCurrentState currentState,
        ExtensionUpdateComparisonIntendedState intendedState,
        ExtensionUpdateRetirementEligibility retirementEligibility)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The Extension Update comparison target kind is not defined.");
        }

        if (!Enum.IsDefined(fingerprintKind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(fingerprintKind),
                fingerprintKind,
                "The Extension Update comparison fingerprint kind is not defined.");
        }

        if (!Enum.IsDefined(currentState))
        {
            throw new ArgumentOutOfRangeException(
                nameof(currentState),
                currentState,
                "The Extension Update current comparison state is not defined.");
        }

        if (!Enum.IsDefined(intendedState))
        {
            throw new ArgumentOutOfRangeException(
                nameof(intendedState),
                intendedState,
                "The Extension Update intended comparison state is not defined.");
        }

        if (!Enum.IsDefined(retirementEligibility))
        {
            throw new ArgumentOutOfRangeException(
                nameof(retirementEligibility),
                retirementEligibility,
                "The Extension Update retirement eligibility is not defined.");
        }

        Path = path;
        PackageId = packageId;
        Kind = kind;
        Region = region;
        SourceAssetPath = sourceAssetPath;
        FingerprintKind = fingerprintKind;
        BaselineFingerprint = baselineFingerprint;
        CurrentFingerprint = currentFingerprint;
        IntendedFingerprint = intendedFingerprint;
        CurrentState = currentState;
        IntendedState = intendedState;
        RetirementEligibility = retirementEligibility;
    }

    internal string Path { get; }

    internal string PackageId { get; }

    internal ExtensionUpdateComparisonTargetKind Kind { get; }

    internal string? Region { get; }

    internal string? SourceAssetPath { get; }

    internal ExtensionUpdateComparisonFingerprintKind FingerprintKind { get; }

    internal string? BaselineFingerprint { get; }

    internal string? CurrentFingerprint { get; }

    internal string? IntendedFingerprint { get; }

    internal ExtensionUpdateComparisonCurrentState CurrentState { get; }

    internal ExtensionUpdateComparisonIntendedState IntendedState { get; }

    internal ExtensionUpdateRetirementEligibility RetirementEligibility { get; }
}

internal sealed record ExtensionUpdateGeneratedRegion
{
    internal ExtensionUpdateGeneratedRegion(
        string path,
        ExtensionUpdateGeneratedRegionState state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Extension Update generated-region state is not defined.");
        }

        Path = path;
        State = state;
    }

    internal string Path { get; }

    internal ExtensionUpdateGeneratedRegionState State { get; }
}

internal sealed record ExtensionUpdateGeneratedNavigation
{
    internal ExtensionUpdateGeneratedNavigation(
        IEnumerable<ExtensionUpdateGeneratedRegion> regions)
    {
        ArgumentNullException.ThrowIfNull(regions);
        Regions = new ReadOnlyCollection<ExtensionUpdateGeneratedRegion>([.. regions
            .Select(value => value ?? throw new ArgumentException(
                "Extension Update generated-navigation regions cannot contain null members.",
                nameof(regions)))]);
    }

    internal IReadOnlyList<ExtensionUpdateGeneratedRegion> Regions { get; }
}

internal sealed record ExtensionUpdateLifecycle
{
    internal ExtensionUpdateLifecycle(
        ExtensionUpdateLifecycleTrust trust,
        ExtensionUpdateLifecycleCoverage coverage,
        ExtensionUpdateLifecycleAction action,
        ExtensionUpdateLifecycleOutcome outcome)
    {
        if (!Enum.IsDefined(trust))
        {
            throw new ArgumentOutOfRangeException(nameof(trust), trust, "The Extension Update lifecycle trust is not defined.");
        }
        if (!Enum.IsDefined(coverage))
        {
            throw new ArgumentOutOfRangeException(nameof(coverage), coverage, "The Extension Update lifecycle coverage is not defined.");
        }
        if (!Enum.IsDefined(action))
        {
            throw new ArgumentOutOfRangeException(nameof(action), action, "The Extension Update lifecycle action is not defined.");
        }
        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The Extension Update lifecycle outcome is not defined.");
        }

        Trust = trust;
        Coverage = coverage;
        Action = action;
        Outcome = outcome;
    }

    internal ExtensionUpdateLifecycleTrust Trust { get; }

    internal ExtensionUpdateLifecycleCoverage Coverage { get; }

    internal ExtensionUpdateLifecycleAction Action { get; }

    internal ExtensionUpdateLifecycleOutcome Outcome { get; }
}

internal sealed record ExtensionUpdateRecovery
{
    internal ExtensionUpdateRecovery(
        ExtensionUpdateRecoveryState state,
        IEnumerable<string> protectedPaths,
        string? residualPath)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Extension Update recovery state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(protectedPaths);
        var paths = protectedPaths
            .Select(value => value ?? throw new ArgumentException(
                "Extension Update recovery paths cannot contain null members.",
                nameof(protectedPaths)))
            .ToArray();
        if (paths.Any(string.IsNullOrWhiteSpace)
            || paths.Distinct(StringComparer.Ordinal).Count() != paths.Length)
        {
            throw new ArgumentException(
                "Extension Update recovery paths must be unique and non-empty.",
                nameof(protectedPaths));
        }

        if (state == ExtensionUpdateRecoveryState.Retained
            && string.IsNullOrWhiteSpace(residualPath))
        {
            throw new ArgumentException(
                "A retained Extension Update recovery bundle requires its residual path.",
                nameof(residualPath));
        }

        if (state is ExtensionUpdateRecoveryState.NotRequired
            or ExtensionUpdateRecoveryState.NotCreated
            or ExtensionUpdateRecoveryState.Removed
            && residualPath is not null)
        {
            throw new ArgumentException(
                "A non-retained Extension Update recovery state cannot expose a residual path.",
                nameof(residualPath));
        }

        State = state;
        ProtectedPaths = new ReadOnlyCollection<string>(paths);
        ResidualPath = residualPath;
    }

    internal ExtensionUpdateRecoveryState State { get; }

    internal IReadOnlyList<string> ProtectedPaths { get; }

    internal string? ResidualPath { get; }
}

internal sealed record ExtensionUpdateVerification
{
    internal ExtensionUpdateVerification(
        ExtensionUpdateVerificationState targets,
        ExtensionUpdateVerificationState topology,
        ExtensionUpdateVerificationState extensionsLifecycle)
    {
        if (!Enum.IsDefined(targets))
        {
            throw new ArgumentOutOfRangeException(nameof(targets), targets, "The Extension Update target verification state is not defined.");
        }
        if (!Enum.IsDefined(topology))
        {
            throw new ArgumentOutOfRangeException(nameof(topology), topology, "The Extension Update topology verification state is not defined.");
        }
        if (!Enum.IsDefined(extensionsLifecycle))
        {
            throw new ArgumentOutOfRangeException(nameof(extensionsLifecycle), extensionsLifecycle, "The Extension Update lifecycle verification state is not defined.");
        }

        Targets = targets;
        Topology = topology;
        ExtensionsLifecycle = extensionsLifecycle;
    }

    internal ExtensionUpdateVerificationState Targets { get; }

    internal ExtensionUpdateVerificationState Topology { get; }

    internal ExtensionUpdateVerificationState ExtensionsLifecycle { get; }
}

internal sealed record ExtensionUpdateFinding
{
    internal ExtensionUpdateFinding(
        ExtensionUpdateFindingCode code,
        string cause,
        string? target = null)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(nameof(code), code, "The Extension Update finding code is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        if (target is not null && target.Length == 0)
        {
            throw new ArgumentException("An Extension Update finding target cannot be empty.", nameof(target));
        }

        Code = code;
        Status = ExtensionUpdateDefinitions.ReadStatus(code);
        Target = target;
        Cause = cause;
    }

    internal ExtensionUpdateFindingCode Code { get; }

    internal CliSemanticStatus Status { get; }

    internal string? Target { get; }

    internal string Cause { get; }
}

internal sealed record ExtensionUpdateResultFacts
{
    internal required ExtensionUpdateSelection? Selection { get; init; }

    internal required ExtensionUpdateSource? Source { get; init; }

    internal required IReadOnlyList<ExtensionUpdatePackage> Packages { get; init; }

    internal required IReadOnlyList<ExtensionUpdateComparison> Comparisons { get; init; }

    internal required ExtensionUpdateGeneratedNavigation? GeneratedNavigation { get; init; }

    internal required IReadOnlyList<ExtensionUpdateEffect> Effects { get; init; }

    internal WorkspacePermissionResult Permissions { get; init; } = WorkspacePermissionResult.NotEvaluated;

    internal required ExtensionUpdateLifecycle Lifecycle { get; init; }

    internal required ExtensionUpdateRecovery Recovery { get; init; }

    internal required ExtensionUpdateVerification Verification { get; init; }
}

internal sealed record ExtensionUpdateResultFormation
{
    internal required CliWorkspace? Workspace { get; init; }

    internal required ExtensionUpdateMode Mode { get; init; }

    internal required bool Force { get; init; }

    internal required bool Prune { get; init; }

    internal required bool Automatic { get; init; }

    internal required ExtensionUpdateResultFacts Facts { get; init; }

    internal required IReadOnlyList<ExtensionUpdateFinding> Findings { get; init; }
}
