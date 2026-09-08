using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Planning;

internal enum RepairDependencyDomain
{
    WorkspaceContainment,
    RouteAndHeading,
    LocalReference,
    LibraryRecord,
    LibraryResidual,
}

internal sealed record RepairDependency
{
    internal RepairDependency(IEnumerable<RepairDependencyDomain> domains)
    {
        ArgumentNullException.ThrowIfNull(domains);
        var values = domains
            .Select(domain =>
            {
                if (!Enum.IsDefined(domain))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(domains),
                        domain,
                        "The Repair diagnosis dependency domain is not defined.");
                }

                return domain;
            })
            .Distinct()
            .ToArray();
        if (values.Length == 0)
        {
            throw new ArgumentException(
                "A Repair step requires at least one diagnosis dependency domain.",
                nameof(domains));
        }

        Domains = new ReadOnlyCollection<RepairDependencyDomain>(values);
    }

    internal IReadOnlyList<RepairDependencyDomain> Domains { get; }
}

internal enum RepairVerificationKind
{
    DestinationLiteral,
    SameTargetIdentity,
    ResultingBytes,
    NoFollowIdentity,
    PriorState,
}

internal sealed record RepairVerificationRequirement
{
    internal RepairVerificationRequirement(IEnumerable<RepairVerificationKind> kinds)
    {
        ArgumentNullException.ThrowIfNull(kinds);
        var values = kinds
            .Select(kind =>
            {
                if (!Enum.IsDefined(kind))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(kinds),
                        kind,
                        "The Repair verification kind is not defined.");
                }

                return kind;
            })
            .Distinct()
            .ToArray();
        if (values.Length == 0)
        {
            throw new ArgumentException(
                "A Repair step requires at least one verification kind.",
                nameof(kinds));
        }

        Kinds = new ReadOnlyCollection<RepairVerificationKind>(values);
    }

    internal IReadOnlyList<RepairVerificationKind> Kinds { get; }
}

internal enum RepairRecoveryRequirementKind
{
    Required,
    NotRequired,
}

internal sealed record RepairRecoveryRequirement
{
    internal RepairRecoveryRequirement(
        RepairRecoveryRequirementKind kind,
        RecoveryBundleAttribution? attribution)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The Repair recovery requirement kind is not defined.");
        }

        if (kind == RepairRecoveryRequirementKind.Required && attribution is null)
        {
            throw new ArgumentException(
                "A required Repair recovery boundary must carry external attribution.",
                nameof(attribution));
        }

        if (kind == RepairRecoveryRequirementKind.Required)
        {
            RepairDefinitions.ValidateRepairRecoveryAttribution(
                attribution ?? throw new ArgumentException("Required Repair recovery attribution is missing.", nameof(attribution)),
                nameof(attribution));
        }
        else if (attribution is not null)
        {
            throw new ArgumentException(
                "A no-recovery Repair boundary cannot carry external attribution.",
                nameof(attribution));
        }

        Kind = kind;
        Attribution = attribution;
    }

    internal RepairRecoveryRequirementKind Kind { get; }

    internal RecoveryBundleAttribution? Attribution { get; }

    internal static RepairRecoveryRequirement Required(RecoveryBundleAttribution attribution)
        => new(RepairRecoveryRequirementKind.Required, attribution);

    internal static RepairRecoveryRequirement NotRequired { get; } = new(
        RepairRecoveryRequirementKind.NotRequired,
        attribution: null);
}

internal enum RepairStepOutcome
{
    Planned,
    NoOp,
    Applied,
    Verified,
    Blocked,
    Failed,
    Interrupted,
}

internal sealed record RepairStep
{
    internal RepairStep(
        int ordinal,
        RepairSelectedProposal selectedProposal,
        RepairDependency dependency,
        RepairVerificationRequirement verification,
        RepairRecoveryRequirement recovery,
        RepairEffect? effect,
        RepairNoOp? noOp,
        RepairStepOutcome outcome)
    {
        if (ordinal < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ordinal),
                ordinal,
                "A Repair step ordinal must be positive.");
        }

        ArgumentNullException.ThrowIfNull(selectedProposal);
        ArgumentNullException.ThrowIfNull(dependency);
        ArgumentNullException.ThrowIfNull(verification);
        ArgumentNullException.ThrowIfNull(recovery);
        if (effect is not null && noOp is not null)
        {
            throw new ArgumentException(
                "A Repair step cannot carry both an effect and a no-op.",
                nameof(noOp));
        }

        if (effect is not null)
        {
            if (recovery.Kind != RepairRecoveryRequirementKind.Required
                || recovery.Attribution is null)
            {
                throw new ArgumentException(
                    "A Repair effect requires a matching required recovery boundary.",
                    nameof(recovery));
            }

            if (!recovery.Attribution.Equals(effect.RecoveryAttribution))
            {
                throw new ArgumentException(
                    "A Repair effect recovery attribution must match its step requirement.",
                    nameof(recovery));
            }
        }

        if (noOp is not null && recovery.Kind != RepairRecoveryRequirementKind.NotRequired)
        {
            throw new ArgumentException(
                "A verified Repair no-op cannot require recovery.",
                nameof(recovery));
        }

        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "The Repair step outcome is not defined.");
        }

        ValidateOutcomeCoherence(outcome, effect, noOp, recovery);

        Ordinal = ordinal;
        SelectedProposal = selectedProposal;
        Dependency = dependency;
        Verification = verification;
        Recovery = recovery;
        Effect = effect;
        NoOp = noOp;
        Outcome = outcome;
    }

    internal int Ordinal { get; }

    internal RepairSelectedProposal SelectedProposal { get; }

    internal RepairProposal Proposal => SelectedProposal.Proposal;

    internal RepairProposalResolution Resolution => SelectedProposal.Resolution;

    internal IReadOnlyList<RepairSelectionOrigin> Origins => SelectedProposal.Origins;

    internal RepairDependency Dependency { get; }

    internal RepairVerificationRequirement Verification { get; }

    internal RepairRecoveryRequirement Recovery { get; }

    internal RepairEffect? Effect { get; }

    internal RepairNoOp? NoOp { get; }

    internal RepairStepOutcome Outcome { get; }

    internal RepairTargetSelection Target => Resolution.Target;

    internal FileStateSnapshot? ExpectedState => Effect?.ExpectedState ?? NoOp?.CurrentState;

    internal FileStateSnapshot? IntendedState => Effect?.IntendedState;

    private static void ValidateOutcomeCoherence(
        RepairStepOutcome outcome,
        RepairEffect? effect,
        RepairNoOp? noOp,
        RepairRecoveryRequirement recovery)
    {
        var hasNoRecovery = recovery.Kind == RepairRecoveryRequirementKind.NotRequired
            && recovery.Attribution is null;
        var hasRequiredRecovery = recovery.Kind == RepairRecoveryRequirementKind.Required
            && recovery.Attribution is not null;
        var coherent = outcome switch
        {
            RepairStepOutcome.NoOp => effect is null && noOp is not null && hasNoRecovery,
            RepairStepOutcome.Planned
                or RepairStepOutcome.Applied
                or RepairStepOutcome.Verified
                => effect is not null && noOp is null && hasRequiredRecovery,
            RepairStepOutcome.Failed
                or RepairStepOutcome.Interrupted
                => noOp is null
                    && (effect is null && hasNoRecovery || effect is not null && hasRequiredRecovery),
            RepairStepOutcome.Blocked => effect is null && noOp is null && hasNoRecovery,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The Repair step outcome is not defined."),
        };
        if (!coherent)
        {
            throw new ArgumentException(
                "The Repair step outcome, action, and recovery requirement are not coherent.",
                nameof(outcome));
        }
    }
}

internal enum RepairConflictKind
{
    OverlappingChanges,
    ExpectedStateMismatch,
    TargetIdentityMismatch,
    UnsafeBoundary,
    MissingAuthority,
    IncompleteFacts,
}

internal sealed record RepairConflict
{
    internal RepairConflict(RepairConflictKind kind, LibraryResidualEvidence library, string cause)
        : this(kind, sourceCanonicalPath: null, occurrence: null, cause)
    {
        ArgumentNullException.ThrowIfNull(library);
        Library = library;
    }

    internal LibraryResidualEvidence? Library { get; }

    internal RepairConflict(
        RepairConflictKind kind,
        string? sourceCanonicalPath,
        SourceLocation? occurrence,
        string cause)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The Repair conflict kind is not defined.");
        }

        if (sourceCanonicalPath is not null)
        {
            SourceCanonicalPath = SourceWorkspaceRelativePath.ValidateMarkdown(
                sourceCanonicalPath,
                nameof(sourceCanonicalPath));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        Kind = kind;
        Occurrence = occurrence;
        Cause = cause;
    }

    internal RepairConflictKind Kind { get; }

    internal string? SourceCanonicalPath { get; }

    internal SourceLocation? Occurrence { get; }

    internal string Cause { get; }
}

internal sealed record RepairPlan
{
    internal RepairPlan(
        RepairRequest request,
        RepairSelection selection,
        IEnumerable<RepairStep> steps,
        IEnumerable<RepairConflict> conflicts,
        ImmutableArray<RepairLibraryRecoveryStep> librarySteps)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(steps);
        ArgumentNullException.ThrowIfNull(conflicts);

        var stepValues = steps
            .Select(step => step ?? throw new ArgumentException(
                "Repair plan steps cannot contain null members.",
                nameof(steps)))
            .OrderBy(step => step.Ordinal)
            .ToArray();
        if (stepValues.Select(step => step.Ordinal).Distinct().Count() != stepValues.Length)
        {
            throw new ArgumentException(
                "Repair plan step ordinals must be unique.",
                nameof(steps));
        }

        ValidateSelectedSteps(selection, stepValues, nameof(steps));
        if (librarySteps.IsDefault || librarySteps.Any(step => step is null)
            || librarySteps.Length != selection.Libraries.Selected.Length
            || librarySteps.Any(step => !selection.Libraries.Selected.Contains(step.Selection))
            || librarySteps.Select(step => step.Selection).Distinct().Count() != librarySteps.Length
            || librarySteps.Any(step => step.Dependency is null || step.Verification is null || !Enum.IsDefined(step.Outcome)
                || step.Effect is { } effect && effect.Selection != step.Selection))
        {
            throw new ArgumentException("Library plan steps must retain every exact selected residual and its typed outcome.", nameof(librarySteps));
        }

        var ordinals = stepValues.Select(step => step.Ordinal).Concat(librarySteps.Select(step => step.Ordinal)).ToArray();
        if (ordinals.Any(ordinal => ordinal < 0) || ordinals.Distinct().Count() != ordinals.Length)
        {
            throw new ArgumentException("Atomic Repair steps require unique nonnegative ordinals across both effect kinds.", nameof(librarySteps));
        }

        LibrarySteps = librarySteps;

        Request = request;
        Selection = selection;
        Steps = new ReadOnlyCollection<RepairStep>(stepValues);
        var effects = new List<RepairEffect>();
        foreach (var effect in stepValues.Select(step => step.Effect).OfType<RepairEffect>())
        {
            if (!effects.Any(value => ReferenceEquals(value, effect)))
            {
                effects.Add(effect);
            }
        }

        Effects = new ReadOnlyCollection<RepairEffect>(effects);
        NoOps = new ReadOnlyCollection<RepairNoOp>([.. stepValues
            .Select(step => step.NoOp)
            .OfType<RepairNoOp>()]);
        Conflicts = new ReadOnlyCollection<RepairConflict>([.. conflicts
            .Select(conflict => conflict ?? throw new ArgumentException(
                "Repair plan conflicts cannot contain null members.",
                nameof(conflicts)))]);
    }

    internal RepairRequest Request { get; }

    internal RepairSelection Selection { get; }

    internal ImmutableArray<RepairLibraryRecoveryStep> LibrarySteps { get; }

    internal IReadOnlyList<RepairStep> Steps { get; }

    internal IReadOnlyList<RepairEffect> Effects { get; }

    internal IReadOnlyList<RepairNoOp> NoOps { get; }

    internal IReadOnlyList<RepairConflict> Conflicts { get; }

    internal bool IsBlocked
        => Conflicts.Count != 0
            || Steps.Any(step => step.Outcome == RepairStepOutcome.Blocked)
            || LibrarySteps.Any(step => step.Outcome == RepairStepOutcome.Blocked);

    internal bool IsNoOp
        => !IsBlocked
            && LibrarySteps.IsEmpty
            && ((Selection.Selected.Count == 0 && Steps.Count == 0)
                || Steps.Count != 0
                    && Steps.All(step => step.Outcome == RepairStepOutcome.NoOp && step.NoOp is not null));

    private static void ValidateSelectedSteps(
        RepairSelection selection,
        RepairStep[] steps,
        string parameterName)
    {
        if (steps.Length != selection.Selected.Count)
        {
            throw new ArgumentException(
                "A Repair plan must contain exactly one step for every selected proposal.",
                parameterName);
        }

        for (var index = 0; index < steps.Length; index++)
        {
            var selectedProposal = steps[index].SelectedProposal;
            if (steps.Take(index).Any(step => ReferenceEquals(step.SelectedProposal, selectedProposal)))
            {
                throw new ArgumentException(
                    "Repair plan steps cannot repeat one selected proposal object.",
                    parameterName);
            }

            if (!selection.Selected.Any(value => ReferenceEquals(value, selectedProposal)))
            {
                throw new ArgumentException(
                    "Every Repair plan step must bind to the exact selected proposal object.",
                    parameterName);
            }
        }

        if (selection.Selected.Any(value => !steps.Any(step => ReferenceEquals(step.SelectedProposal, value))))
        {
            throw new ArgumentException(
                "Every selected Repair proposal must have exactly one plan step.",
                parameterName);
        }
    }
}
