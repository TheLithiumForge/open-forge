using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Result;

internal enum RepairCoverageState
{
    NotRequested,
    Complete,
    Incomplete,
    Blocked,
}

internal enum RepairPreflightState
{
    NotRequested,
    Ready,
    Incomplete,
    Blocked,
}

internal enum RepairApplicationState
{
    NotRequested,
    Confirmed,
    NotStarted,
    Applied,
    Failed,
    Interrupted,
    Unknown,
}

internal enum RepairVerificationState
{
    NotRequested,
    Planned,
    Verified,
    Failed,
    Unknown,
}

internal enum RepairRecoveryState
{
    NotRequired,
    NotCreated,
    Prepared,
    Removed,
    Retained,
    Incomplete,
    Blocked,
    Unknown,
}

internal enum RepairResidualState
{
    None,
    Retained,
    Unknown,
}

internal enum RepairPostDiagnosisState
{
    NotRequested,
    Complete,
    Incomplete,
    Blocked,
    Failed,
}

internal sealed record RepairDiagnosisCoverage
{
    internal RepairDiagnosisCoverage(
        RepairCoverageState workspaceAndPath,
        RepairCoverageState routeAndHeading,
        RepairCoverageState localReferences,
        RepairCoverageState selectedScope)
    {
        WorkspaceAndPath = Validate(workspaceAndPath, nameof(workspaceAndPath));
        RouteAndHeading = Validate(routeAndHeading, nameof(routeAndHeading));
        LocalReferences = Validate(localReferences, nameof(localReferences));
        SelectedScope = Validate(selectedScope, nameof(selectedScope));
    }

    internal RepairCoverageState WorkspaceAndPath { get; }

    internal RepairCoverageState RouteAndHeading { get; }

    internal RepairCoverageState LocalReferences { get; }

    internal RepairCoverageState SelectedScope { get; }

    private static RepairCoverageState Validate(RepairCoverageState value, string name)
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(name, value, "The Repair coverage state is not defined.");
        }

        return value;
    }
}

internal sealed record RepairCounts
{
    internal RepairCounts(
        int selectedFindings,
        int unselectedFindings,
        int repaired,
        int remaining,
        int newFindings,
        int manual,
        int guided,
        int blocked,
        int selectedEffects,
        int appliedEffects,
        int verifiedEffects,
        int noOps,
        int conflicts)
    {
        var values = new[]
        {
            selectedFindings,
            unselectedFindings,
            repaired,
            remaining,
            newFindings,
            manual,
            guided,
            blocked,
            selectedEffects,
            appliedEffects,
            verifiedEffects,
            noOps,
            conflicts,
        };
        if (values.Any(value => value < 0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(selectedFindings),
                "Repair result counts cannot be negative.");
        }

        SelectedFindings = selectedFindings;
        UnselectedFindings = unselectedFindings;
        Repaired = repaired;
        Remaining = remaining;
        NewFindings = newFindings;
        Manual = manual;
        Guided = guided;
        Blocked = blocked;
        SelectedEffects = selectedEffects;
        AppliedEffects = appliedEffects;
        VerifiedEffects = verifiedEffects;
        NoOps = noOps;
        Conflicts = conflicts;
    }

    internal int SelectedFindings { get; }

    internal int UnselectedFindings { get; }

    internal int Repaired { get; }

    internal int Remaining { get; }

    internal int NewFindings { get; }

    internal int Manual { get; }

    internal int Guided { get; }

    internal int Blocked { get; }

    internal int SelectedEffects { get; }

    internal int AppliedEffects { get; }

    internal int VerifiedEffects { get; }

    internal int NoOps { get; }

    internal int Conflicts { get; }

    internal static RepairCounts Empty { get; } = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
}

internal sealed record RepairPreflight
{
    internal RepairPreflight(
        RepairPreflightState state,
        string? cause,
        RepairRecoveryState recovery)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Repair preflight state is not defined.");
        }

        if (!Enum.IsDefined(recovery))
        {
            throw new ArgumentOutOfRangeException(nameof(recovery), recovery, "The Repair recovery state is not defined.");
        }

        if (state is RepairPreflightState.Incomplete or RepairPreflightState.Blocked)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        }

        State = state;
        Cause = cause;
        Recovery = recovery;
    }

    internal RepairPreflightState State { get; }

    internal string? Cause { get; }

    internal RepairRecoveryState Recovery { get; }

    internal static RepairPreflight NotRequested { get; } = new(
        RepairPreflightState.NotRequested,
        cause: null,
        recovery: RepairRecoveryState.NotRequired);
}

internal sealed record RepairApplication
{
    internal RepairApplication(
        RepairApplicationState state,
        int appliedEffects,
        string? cause)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Repair application state is not defined.");
        }

        if (appliedEffects < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(appliedEffects), appliedEffects, "Applied Repair effects cannot be negative.");
        }

        if (state is RepairApplicationState.Failed
            or RepairApplicationState.Interrupted
            or RepairApplicationState.Unknown)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        }

        State = state;
        AppliedEffects = appliedEffects;
        Cause = cause;
    }

    internal RepairApplicationState State { get; }

    internal int AppliedEffects { get; }

    internal string? Cause { get; }

    internal static RepairApplication NotRequested { get; } = new(
        RepairApplicationState.NotRequested,
        0,
        cause: null);
}

internal sealed record RepairVerification
{
    internal RepairVerification(
        RepairVerificationState targets,
        RepairVerificationState resultingBytes,
        RepairVerificationState postConditions)
        : this(targets, resultingBytes, postConditions, [])
    {
    }

    internal RepairVerification(
        RepairVerificationState targets,
        RepairVerificationState resultingBytes,
        RepairVerificationState postConditions,
        IEnumerable<RepairEffectVerification> effects)
    {
        Effects = new ReadOnlyCollection<RepairEffectVerification>([.. effects]);
        Targets = Validate(targets, nameof(targets));
        ResultingBytes = Validate(resultingBytes, nameof(resultingBytes));
        PostConditions = Validate(postConditions, nameof(postConditions));
    }

    internal IReadOnlyList<RepairEffectVerification> Effects { get; }

    internal RepairVerificationState Targets { get; }

    internal RepairVerificationState ResultingBytes { get; }

    internal RepairVerificationState PostConditions { get; }

    internal static RepairVerification NotRequested { get; } = new(
        RepairVerificationState.NotRequested,
        RepairVerificationState.NotRequested,
        RepairVerificationState.NotRequested);

    private static RepairVerificationState Validate(RepairVerificationState value, string name)
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(name, value, "The Repair verification state is not defined.");
        }

        return value;
    }
}

internal sealed record RepairRecovery
{
    internal RepairRecovery(
        RepairRecoveryState state,
        RepairResidualState residual,
        string? residualPath,
        RecoveryBundleAttribution? attribution)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Repair recovery state is not defined.");
        }

        if (!Enum.IsDefined(residual))
        {
            throw new ArgumentOutOfRangeException(nameof(residual), residual, "The Repair residual state is not defined.");
        }

        var coherent = state switch
        {
            RepairRecoveryState.NotRequired
                or RepairRecoveryState.NotCreated
                or RepairRecoveryState.Removed
                => residual == RepairResidualState.None && residualPath is null,
            RepairRecoveryState.Prepared
                or RepairRecoveryState.Retained
                => residual == RepairResidualState.Retained && residualPath is not null,
            RepairRecoveryState.Incomplete
                or RepairRecoveryState.Blocked
                    => residual == RepairResidualState.Unknown
                    || (residual == RepairResidualState.Retained && residualPath is not null),
            RepairRecoveryState.Unknown => residual == RepairResidualState.Unknown,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Repair recovery state is not defined."),
        };
        if (!coherent)
        {
            throw new ArgumentException(
                "The Repair recovery state, residual, and path do not describe one coherent lifecycle fact.",
                nameof(residualPath));
        }

        if (residualPath is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(residualPath);
            if (!Path.IsPathFullyQualified(residualPath)
                || !string.Equals(
                    Path.GetFullPath(residualPath),
                    residualPath,
                    StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "A Repair recovery residual path must be fully qualified and normalized.",
                    nameof(residualPath));
            }
        }

        if ((state is RepairRecoveryState.NotRequired or RepairRecoveryState.NotCreated)
            && attribution is not null)
        {
            throw new ArgumentException(
                "A Repair recovery without a prepared bundle cannot carry attribution.",
                nameof(attribution));
        }

        if ((state is RepairRecoveryState.Prepared
                or RepairRecoveryState.Retained
                or RepairRecoveryState.Removed)
            && attribution is null)
        {
            throw new ArgumentException(
                "A valid Repair recovery bundle requires its exact external attribution.",
                nameof(attribution));
        }

        if (attribution is not null)
        {
            RepairDefinitions.ValidateRepairRecoveryAttribution(
                attribution,
                nameof(attribution));
        }

        State = state;
        Residual = residual;
        ResidualPath = residualPath;
        Attribution = attribution;
    }

    internal RepairRecoveryState State { get; }

    internal RepairResidualState Residual { get; }

    internal string? ResidualPath { get; }

    internal RecoveryBundleAttribution? Attribution { get; }

    internal static RepairRecovery NotRequired { get; } = new(
        RepairRecoveryState.NotRequired,
        RepairResidualState.None,
        residualPath: null,
        attribution: null);
}

internal sealed record RepairPostDiagnosis
{
    internal RepairPostDiagnosis(
        RepairPostDiagnosisState state,
        RepairDiagnosisCoverage coverage,
        IEnumerable<RepairFinding> findings)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Repair post-diagnosis state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(coverage);
        ArgumentNullException.ThrowIfNull(findings);
        State = state;
        Coverage = coverage;
        Findings = new ReadOnlyCollection<RepairFinding>([.. findings
            .Select(value => value ?? throw new ArgumentException(
                "Repair post-diagnosis findings cannot contain null members.",
                nameof(findings)))]);
    }

    internal RepairPostDiagnosisState State { get; }

    internal RepairDiagnosisCoverage Coverage { get; }

    internal IReadOnlyList<RepairFinding> Findings { get; }

    internal static RepairPostDiagnosis NotRequested { get; } = new(
        RepairPostDiagnosisState.NotRequested,
        new RepairDiagnosisCoverage(
            RepairCoverageState.NotRequested,
            RepairCoverageState.NotRequested,
            RepairCoverageState.NotRequested,
            RepairCoverageState.NotRequested),
        []);
}
