using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;

internal sealed record CleanupPlanEntry
{
    private CleanupPlanEntry(
        int ordinal,
        CleanupCandidate candidate,
        CleanupEffectCondition resultEffect)
    {
        if (ordinal < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ordinal),
                ordinal,
                "The Cleanup plan entry ordinal cannot be negative.");
        }

        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(resultEffect);
        resultEffect.ValidateFor(candidate.Action);

        Ordinal = ordinal;
        Candidate = candidate;
        ResultEffect = resultEffect;
    }

    internal static CleanupPlanEntry Create(
        int ordinal,
        CleanupCandidate candidate,
        CleanupEffectCondition resultEffect)
        => new(ordinal, candidate, resultEffect);

    public int Ordinal { get; }

    public CleanupCandidate Candidate { get; }

    public string Path => Candidate.Path;

    public RecoveryBundleCandidateKind Kind => Candidate.Kind;

    internal Enum KindValue => Kind;

    internal Enum IntegrityValue => Integrity;

    public RecoveryBundleIntegrity Integrity => Candidate.Integrity;

    public CleanupArtifactFileKind FileKind => Candidate.FileKind;

    public CleanupWorkspaceAssociation WorkspaceAssociation => Candidate.WorkspaceAssociation;

    public CleanupRecoveryProvenance? Provenance => Candidate.Provenance;

    public string? Cause => Candidate.Cause;

    public CleanupCandidateEligibility Eligibility => Candidate.Eligibility;

    public CleanupPlanAction Action => Candidate.Action;

    public CleanupLeaseBoundary LeaseBoundary => Candidate.LeaseBoundary;

    public CleanupVerificationCondition Verification => Candidate.Verification;

    public CleanupEffectCondition ResultEffect { get; }
}

internal sealed record CleanupPlan
{
    private CleanupPlan(
        CleanupRequest request,
        CleanupCatalogue catalogue,
        ImmutableArray<CleanupPlanEntry> entries,
        ImmutableArray<CleanupPlanEntry> deletionEntries,
        CleanupPlanSafety safety)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(catalogue);

        ValidateEnum(safety, nameof(safety), "plan safety");
        ValidateMaterialized(entries, nameof(entries));
        ValidateMaterialized(deletionEntries, nameof(deletionEntries));
        ValidateEntryCorrespondence(catalogue, entries);
        ValidateDeletionEntries(deletionEntries);
        ValidateSelectedWorkspace(request, catalogue);
        ValidateSafety(catalogue, entries, deletionEntries, safety);

        Request = request;
        Catalogue = catalogue;
        Entries = entries;
        DeletionEntries = deletionEntries;
        Safety = safety;
    }

    internal static CleanupPlan Create(
        CleanupRequest request,
        CleanupCatalogue catalogue,
        ImmutableArray<CleanupPlanEntry> entries,
        ImmutableArray<CleanupPlanEntry> deletionEntries,
        CleanupPlanSafety safety)
        => new(request, catalogue, entries, deletionEntries, safety);

    private CleanupPlan()
    {
        Request = null;
        Catalogue = CleanupCatalogue.Create(CleanupCatalogueCoverage.NotEstablished, []);
        Entries = [];
        DeletionEntries = [];
        Safety = CleanupPlanSafety.NotEstablished;
    }

    private CleanupPlan(CleanupRequest request, ImmutableArray<CleanupPlan> stores)
    {
        if (stores.IsDefaultOrEmpty || stores.Any(store => store.Request != request))
        {
            throw new ArgumentException("Store plans must belong to the selected request.", nameof(stores));
        }

        Request = request;
        var coverage = stores.All(store => store.Catalogue.Coverage == CleanupCatalogueCoverage.Complete)
            ? CleanupCatalogueCoverage.Complete : CleanupCatalogueCoverage.Incomplete;
        if (stores.Any(store => store.Catalogue.Coverage == CleanupCatalogueCoverage.Interrupted))
        {
            coverage = CleanupCatalogueCoverage.Interrupted;
        }

        Catalogue = CleanupCatalogue.Create(coverage,
            [.. stores.SelectMany(store => store.Catalogue.Candidates).OrderBy(candidate => candidate.Path, StringComparer.Ordinal)]);
        Entries = [.. Catalogue.Candidates.Select((candidate, ordinal) => CleanupPlanEntry.Create(
            ordinal, candidate, stores.SelectMany(store => store.Entries).Single(entry => ReferenceEquals(entry.Candidate, candidate)).ResultEffect))];
        var deletable = stores.SelectMany(store => store.DeletionEntries).Select(entry => entry.Candidate).ToHashSet();
        DeletionEntries = [.. Entries.Where(entry => deletable.Contains(entry.Candidate))];
        Safety = stores.All(store => store.Safety == CleanupPlanSafety.Safe) ? CleanupPlanSafety.Safe : CleanupPlanSafety.Blocked;
    }

    internal static CleanupPlan Aggregate(CleanupRequest request, ImmutableArray<CleanupPlan> stores)
        => new(request, stores);

    internal static CleanupPlan NotEstablished()
        => new();

    public CleanupRequest? Request { get; }

    public CleanupCatalogue Catalogue { get; }

    public ImmutableArray<CleanupPlanEntry> Entries { get; }

    public ImmutableArray<CleanupPlanEntry> DeletionEntries { get; }

    public CleanupPlanSafety Safety { get; }

    internal static bool IsSafeEntry(CleanupPlanEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return IsDeletionEntry(entry) || IsAllowedPreservedMalformedFinal(entry);
    }

    internal static bool IsDeletionEntry(CleanupPlanEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return entry.Eligibility == CleanupCandidateEligibility.Eligible
            && entry.Action == CleanupPlanAction.Delete;
    }

    private static bool IsAllowedPreservedMalformedFinal(CleanupPlanEntry entry)
        => entry.Eligibility == CleanupCandidateEligibility.Blocked
            && entry.Action == CleanupPlanAction.Preserve
            && entry.Kind == RecoveryBundleCandidateKind.Final
            && entry.Integrity == RecoveryBundleIntegrity.Malformed
            && entry.FileKind == CleanupArtifactFileKind.Ordinary
            && entry.Verification.State == CleanupVerificationConditionState.SemanticFinal
            && entry.Verification.ExpectedFileKind == CleanupArtifactFileKind.Ordinary
            && entry.Verification.ExpectedIntegrity == RecoveryBundleIntegrity.Malformed;

    private static void ValidateEntryCorrespondence(
        CleanupCatalogue catalogue,
        ImmutableArray<CleanupPlanEntry> entries)
    {
        if (entries.Length != catalogue.Candidates.Length)
        {
            throw new ArgumentException(
                "Cleanup plan entries must cover the catalogue candidates exactly.",
                nameof(entries));
        }

        for (var index = 0; index < entries.Length; index++)
        {
            var entry = entries[index];
            ArgumentNullException.ThrowIfNull(entry);

            if (entry.Ordinal != index)
            {
                throw new ArgumentException(
                    "Cleanup plan entry ordinals must be nonnegative and contiguous.",
                    nameof(entries));
            }

            if (!ReferenceEquals(entry.Candidate, catalogue.Candidates[index]))
            {
                throw new ArgumentException(
                    "Cleanup plan entries must reference catalogue candidates in order.",
                    nameof(entries));
            }
        }
    }

    private static void ValidateSafety(
        CleanupCatalogue catalogue,
        ImmutableArray<CleanupPlanEntry> entries,
        ImmutableArray<CleanupPlanEntry> deletionEntries,
        CleanupPlanSafety safety)
    {
        switch (safety)
        {
            case CleanupPlanSafety.NotEstablished:
                if (catalogue.Coverage != CleanupCatalogueCoverage.NotEstablished
                    || catalogue.Candidates.Length != 0
                    || entries.Length != 0
                    || deletionEntries.Length != 0)
                {
                    throw new ArgumentException(
                        "A not-established Cleanup plan must be empty and use a not-established catalogue.",
                        nameof(safety));
                }

                break;
            case CleanupPlanSafety.Safe:
                if (catalogue.Coverage != CleanupCatalogueCoverage.Complete
                    || entries.Any(entry => !IsSafeEntry(entry))
                    || deletionEntries.Any(entry => !IsDeletionEntry(entry))
                    || !SameEntries(
                        entries.Where(IsDeletionEntry).ToImmutableArray(),
                        deletionEntries))
                {
                    throw new ArgumentException(
                        "A safe Cleanup plan must cover complete trusted entries and expose only eligible deletion entries.",
                        nameof(safety));
                }

                break;
            case CleanupPlanSafety.Blocked:
                if (!deletionEntries.IsEmpty
                    || (catalogue.Coverage is not (
                        CleanupCatalogueCoverage.Incomplete
                        or CleanupCatalogueCoverage.Interrupted)
                        && !entries.Any(entry =>
                            entry.Eligibility == CleanupCandidateEligibility.Blocked)))
                {
                    throw new ArgumentException(
                        "A blocked Cleanup plan must expose no deletion entries and an incomplete or blocked fact.",
                        nameof(safety));
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(safety),
                    safety,
                    "The Cleanup plan safety is not defined.");
        }
    }

    private static void ValidateDeletionEntries(ImmutableArray<CleanupPlanEntry> deletionEntries)
    {
        for (var index = 0; index < deletionEntries.Length; index++)
        {
            ArgumentNullException.ThrowIfNull(deletionEntries[index]);
        }
    }

    private static void ValidateSelectedWorkspace(
        CleanupRequest request,
        CleanupCatalogue catalogue)
    {
        var selectedPhysicalPath = WorkspaceIdentity.NormalizePhysicalPath(
            request.Workspace.PhysicalRoot);
        var selectedWorkspaceKey = WorkspaceIdentity.Key(selectedPhysicalPath);
        foreach (var candidate in catalogue.Candidates)
        {
            var association = candidate.WorkspaceAssociation;
            if ((association.State is CleanupWorkspaceAssociationState.CurrentWorkspace
                    or CleanupWorkspaceAssociationState.Mismatched
                    or CleanupWorkspaceAssociationState.Unavailable)
                && (!PhysicalIdentityTracker.PathComparer.Equals(
                        association.SelectedPhysicalPath,
                        selectedPhysicalPath)
                    || !string.Equals(
                        association.SelectedWorkspaceKey,
                        selectedWorkspaceKey,
                        StringComparison.Ordinal)))
            {
                throw new ArgumentException(
                    "A Cleanup candidate association must identify the selected workspace.",
                    nameof(request));
            }
        }
    }

    private static bool SameEntries(
        ImmutableArray<CleanupPlanEntry> entries,
        ImmutableArray<CleanupPlanEntry> comparison)
    {
        if (entries.Length != comparison.Length)
        {
            return false;
        }

        for (var index = 0; index < entries.Length; index++)
        {
            if (!ReferenceEquals(entries[index], comparison[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static void ValidateMaterialized<T>(ImmutableArray<T> values, string parameterName)
    {
        if (values.IsDefault)
        {
            throw new ArgumentException(
                "The Cleanup plan collection must be materialized.",
                parameterName);
        }
    }

    private static void ValidateEnum<TEnum>(TEnum value, string parameterName, string description)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, $"The {description} is not defined.");
        }
    }
}
