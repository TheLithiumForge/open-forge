using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Presentation;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Cleanup;

internal static class CleanupTestData
{
    internal static readonly Guid OperationId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    internal static CliWorkspace Workspace(
        string suffix = "contract",
        CliWorkspaceSelectionMethod selectedBy = CliWorkspaceSelectionMethod.ExplicitWorkspace)
    {
        var root = Path.GetFullPath(
            Path.Combine(Path.GetTempPath(), $"open-forge-cleanup-{suffix}"));
        return new CliWorkspace(root, root, selectedBy);
    }

    internal static CliInvocation Invocation(CliWorkspace? workspace = null)
    {
        var selectedWorkspace = workspace ?? Workspace();
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(
                CliOutputFormat.Human,
                CliView.Expanded,
                CliVerbosity.Normal),
            CliTerminalMode.None,
            new CliWorkspaceRequest(null, selectedWorkspace.LexicalRoot),
            selectedWorkspace);
    }

    internal static CleanupRequest Request(
        CliWorkspace? workspace = null,
        CleanupMode mode = CleanupMode.Apply)
        => new(workspace ?? Workspace(), mode);

    internal static CleanupCandidate Candidate(
        RecoveryBundleCandidateKind kind = RecoveryBundleCandidateKind.Final,
        RecoveryBundleIntegrity integrity = RecoveryBundleIntegrity.Verified,
        CliWorkspace? selectedWorkspace = null,
        CliWorkspace? candidateWorkspace = null,
        string? path = null,
        CleanupArtifactFileKind? fileKind = null,
        CleanupWorkspaceAssociationState? associationState = null,
        CleanupLeaseBoundaryState? leaseState = null,
        CleanupCandidateEligibility? eligibility = null,
        CleanupPlanAction? action = null,
        Guid? operationId = null,
        string? cause = null)
    {
        var selected = selectedWorkspace ?? Workspace();
        var candidate = candidateWorkspace ?? selected;
        var candidateOperationId = operationId ?? OperationId;
        var candidatePath = path ?? DefaultCandidatePath(kind, candidateOperationId);

        RecoveryBundleCandidateSnapshot snapshot;
        CleanupRecoveryProvenance? provenance = null;
        if (kind == RecoveryBundleCandidateKind.Draft)
        {
            snapshot = integrity == RecoveryBundleIntegrity.Incomplete
                ? RecoveryBundleCandidateSnapshot.IncompleteDraft(candidatePath)
                : RecoveryBundleCandidateSnapshot.Classified(
                    candidatePath,
                    kind,
                    integrity,
                    "The synthetic Cleanup candidate is unavailable.");
        }
        else if (integrity == RecoveryBundleIntegrity.Verified)
        {
            var verified = VerifiedRead(candidate, candidatePath, candidateOperationId);
            snapshot = RecoveryBundleCandidateSnapshot.VerifiedFinal(verified);
            provenance = Provenance(verified);
        }
        else
        {
            snapshot = RecoveryBundleCandidateSnapshot.Classified(
                candidatePath,
                kind,
                integrity,
                "The synthetic Cleanup candidate is not verified.");
        }

        var resolvedAssociationState = associationState
            ?? ReadAssociationState(selected, candidate);
        var selectedPath = WorkspaceIdentity.NormalizePhysicalPath(selected.PhysicalRoot);
        var selectedKey = WorkspaceIdentity.Key(selectedPath);
        var candidatePathIdentity = WorkspaceIdentity.NormalizePhysicalPath(candidate.PhysicalRoot);
        var candidateKey = WorkspaceIdentity.Key(candidatePathIdentity);
        var workspaceAssociation = resolvedAssociationState switch
        {
            CleanupWorkspaceAssociationState.NotEstablished => CleanupWorkspaceAssociation.Create(
                resolvedAssociationState,
                null,
                null,
                null,
                null),
            CleanupWorkspaceAssociationState.CurrentWorkspace => CleanupWorkspaceAssociation.Create(
                resolvedAssociationState,
                selectedPath,
                selectedPath,
                selectedKey,
                selectedKey),
            CleanupWorkspaceAssociationState.Mismatched => CleanupWorkspaceAssociation.Create(
                resolvedAssociationState,
                selectedPath,
                candidatePathIdentity,
                selectedKey,
                candidateKey),
            CleanupWorkspaceAssociationState.Unavailable => CleanupWorkspaceAssociation.Create(
                resolvedAssociationState,
                selectedPath,
                null,
                selectedKey,
                null),
            _ => throw new ArgumentOutOfRangeException(nameof(associationState)),
        };

        var resolvedLeaseState = leaseState ?? ReadLeaseState(resolvedAssociationState);
        var leaseBoundary = resolvedLeaseState is
            CleanupLeaseBoundaryState.NotEstablished or CleanupLeaseBoundaryState.NotRequested
            ? CleanupLeaseBoundary.Create(resolvedLeaseState, null, null, null)
            : CleanupLeaseBoundary.Create(
                resolvedLeaseState,
                selectedKey,
                CleanupDefinitions.CommandIdentity,
                candidateOperationId);

        var resolvedFileKind = fileKind ?? CleanupArtifactFileKind.Ordinary;
        CleanupVerificationCondition resolvedVerification;
        if (resolvedAssociationState == CleanupWorkspaceAssociationState.NotEstablished)
        {
            resolvedVerification = CleanupVerificationCondition.Create(
                CleanupVerificationConditionState.NotEstablished,
                null,
                CleanupArtifactFileKind.NotEstablished,
                null);
        }
        else
        {
            var verificationState = kind == RecoveryBundleCandidateKind.Final
                ? CleanupVerificationConditionState.SemanticFinal
                : CleanupVerificationConditionState.ExactPathAndKind;
            resolvedVerification = CleanupVerificationCondition.Create(
                verificationState,
                snapshot.Path,
                resolvedFileKind,
                integrity);
        }

        var fullyEligible = resolvedAssociationState == CleanupWorkspaceAssociationState.CurrentWorkspace
            && resolvedLeaseState == CleanupLeaseBoundaryState.Required
            && resolvedFileKind == CleanupArtifactFileKind.Ordinary
            && IsEligibleKindAndIntegrity(kind, integrity);
        var resolvedEligibility = eligibility ?? ReadEligibility(fullyEligible);
        var resolvedAction = action ?? ReadAction(resolvedEligibility);
        var resolvedCause = cause;
        if (resolvedCause is null && resolvedEligibility == CleanupCandidateEligibility.Blocked)
        {
            resolvedCause = "The synthetic Cleanup candidate is blocked.";
        }

        return CleanupCandidate.Create(
            snapshot,
            resolvedFileKind,
            workspaceAssociation,
            leaseBoundary,
            provenance,
            resolvedVerification,
            resolvedEligibility,
            resolvedAction,
            resolvedCause);
    }

    internal static RecoveryBundleVerifiedRead VerifiedRead(
        CliWorkspace workspace,
        string? path = null,
        Guid? operationId = null)
    {
        var id = operationId ?? OperationId;
        var workspacePath = WorkspaceIdentity.NormalizePhysicalPath(workspace.PhysicalRoot);
        return new RecoveryBundleVerifiedRead
        {
            BundlePath = path ?? DefaultCandidatePath(RecoveryBundleCandidateKind.Final, id),
            WorkspacePhysicalPath = workspacePath,
            WorkspaceKey = WorkspaceIdentity.Key(workspacePath),
            Command = "index",
            Attribution = RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Index,
                RecoveryBundleOperation.Index,
                workspace),
            OperationId = id,
            Entries = [],
        };
    }

    internal static CleanupRecoveryProvenance Provenance(RecoveryBundleVerifiedRead verified)
        => new()
        {
            Attribution = verified.Attribution,
            Command = verified.Command,
            WorkspacePhysicalPath = verified.WorkspacePhysicalPath,
            WorkspaceKey = verified.WorkspaceKey,
            OperationId = verified.OperationId,
        };

    internal static CleanupCatalogue Catalogue(
        CleanupCatalogueCoverage coverage = CleanupCatalogueCoverage.Complete,
        params CleanupCandidate[] candidates)
        => CleanupCatalogue.Create(coverage, [.. candidates]);

    internal static CleanupPlan Plan(
        CleanupRequest? request = null,
        CleanupCatalogue? catalogue = null)
    {
        var selectedRequest = request ?? Request();
        var selectedCatalogue = catalogue
            ?? Catalogue(
                candidates:
                [Candidate(selectedWorkspace: selectedRequest.Workspace)]);
        var entries = selectedCatalogue.Candidates
            .Select((candidate, index) => CleanupPlanEntry.Create(
                index,
                candidate,
                CleanupEffectCondition.Create(
                    candidate.Action == CleanupPlanAction.Delete
                        ? CleanupEffectOutcome.Planned
                        : CleanupEffectOutcome.NotStarted,
                    candidate.Action == CleanupPlanAction.Delete
                        ? CleanupEffectResidual.None
                        : CleanupEffectResidual.Retained)))
            .ToImmutableArray();
        CleanupPlanSafety safety;
        if (selectedCatalogue.Coverage == CleanupCatalogueCoverage.NotEstablished)
        {
            safety = CleanupPlanSafety.NotEstablished;
        }
        else if (selectedCatalogue.Coverage == CleanupCatalogueCoverage.Complete
            && entries.All(entry => entry.Eligibility == CleanupCandidateEligibility.Eligible))
        {
            safety = CleanupPlanSafety.Safe;
        }
        else
        {
            safety = CleanupPlanSafety.Blocked;
        }

        ImmutableArray<CleanupPlanEntry> deletionEntries = safety == CleanupPlanSafety.Safe
            ? [.. entries.Where(entry => entry.Action == CleanupPlanAction.Delete)]
            : [];

        return CleanupPlan.Create(
            selectedRequest,
            selectedCatalogue,
            entries,
            deletionEntries,
            safety);
    }

    internal static CleanupEffect Effect(
        CleanupPlanEntry entry,
        CleanupEffectOutcome outcome = CleanupEffectOutcome.Verified,
        CleanupEffectResidual residual = CleanupEffectResidual.None,
        string? cause = null)
        => CleanupEffect.Create(entry, outcome, residual, cause);

    internal static CleanupResidual Residual(
        CleanupEffect effect,
        string cause = "The synthetic Cleanup residual remains observable.")
        => CleanupResidual.Create(effect, cause);

    internal static CleanupResultFacts Facts(
        CleanupPlan? plan = null,
        ImmutableArray<CleanupEffect> effects = default,
        ImmutableArray<CleanupResidual> residuals = default,
        ImmutableArray<CleanupFinding> findings = default)
    {
        var selectedPlan = plan ?? Plan();
        ImmutableArray<CleanupEffect> selectedEffects;
        if (effects.IsDefault)
        {
            selectedEffects = [.. selectedPlan.Entries
                .Select(entry => Effect(
                    entry,
                    entry.Action == CleanupPlanAction.Delete
                        ? CleanupEffectOutcome.Verified
                        : CleanupEffectOutcome.NotStarted,
                    entry.Action == CleanupPlanAction.Delete
                        ? CleanupEffectResidual.None
                        : CleanupEffectResidual.Retained))];
        }
        else
        {
            selectedEffects = effects;
        }

        ImmutableArray<CleanupResidual> selectedResiduals;
        if (residuals.IsDefault)
        {
            selectedResiduals = [.. selectedEffects
                .Where(effect => effect.Residual != CleanupEffectResidual.None)
                .Select(effect => Residual(effect))];
        }
        else
        {
            selectedResiduals = residuals;
        }

        ImmutableArray<CleanupFinding> selectedFindings = findings.IsDefault
            ? []
            : findings;
        var selectedRequest = selectedPlan.Request;
        Assert.NotNull(selectedRequest);
        var isDryRun = selectedRequest.IsDryRun;
        return CleanupResultFacts.Create(
            selectedPlan,
            new CleanupPreflight
            {
                State = CleanupPreflightState.Complete,
                Cause = null,
            },
            new CleanupLease
            {
                State = isDryRun
                    ? CleanupLeaseState.NotRequested
                    : CleanupLeaseState.Acquired,
                Cause = null,
            },
            new CleanupCatalogueComparison
            {
                State = isDryRun
                    ? CleanupCatalogueComparisonState.NotRequested
                    : CleanupCatalogueComparisonState.Matched,
                Planned = isDryRun ? null : selectedPlan.Catalogue,
                Observed = isDryRun ? null : selectedPlan.Catalogue,
                Cause = null,
            },
            selectedEffects,
            selectedResiduals,
            new CleanupVerification
            {
                State = isDryRun
                    ? CleanupVerificationState.NotRequested
                    : CleanupVerificationState.Verified,
                Cause = null,
            },
            selectedFindings);
    }

    internal static CleanupResult Result(
        CleanupResultFacts? facts = null,
        CliSemanticStatus status = CliSemanticStatus.Complete,
        CliWorkspace? workspace = null,
        CliNextAction? next = null)
    {
        var selectedFacts = facts ?? Facts();
        var selectedRequest = selectedFacts.Plan.Request;
        Assert.NotNull(selectedRequest);
        return new CleanupResult
        {
            Status = status,
            Workspace = workspace ?? selectedRequest.Workspace,
            Next = next,
            Facts = selectedFacts,
        };
    }

    internal static CleanupJsonDocument JsonSample(
        bool includeNext = true,
        bool includeWorkspace = true,
        bool includeProvenance = true)
    {
        const string workspacePath = "/tmp/open-forge-cleanup-json-workspace";
        const string workspaceKey = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        const string operationId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var association = new CleanupJsonWorkspaceAssociation
        {
            State = "current-workspace",
            SelectedPhysicalPath = workspacePath,
            CandidatePhysicalPath = workspacePath,
            SelectedWorkspaceKey = workspaceKey,
            CandidateWorkspaceKey = workspaceKey,
            Cause = null,
        };
        var leaseBoundary = new CleanupJsonLeaseBoundary
        {
            State = "required",
            WorkspaceKey = workspaceKey,
            Command = "cleanup",
            OperationId = operationId,
            Cause = null,
        };
        var verificationCondition = new CleanupJsonVerificationCondition
        {
            State = "semantic-final",
            ExpectedPath = "/tmp/open-forge-cleanup-json-recovery/operation-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.zip",
            ExpectedFileKind = "ordinary",
            ExpectedIntegrity = "verified",
            Cause = null,
        };
        var provenance = includeProvenance
            ? new CleanupJsonRecoveryProvenance
            {
                Producer = "index",
                Operation = "index",
                Subject = new CleanupJsonRecoverySubject
                {
                    Kind = "workspace",
                    Identity = workspaceKey,
                },
                Command = "index",
                WorkspacePhysicalPath = workspacePath,
                WorkspaceKey = workspaceKey,
                OperationId = operationId,
            }
            : null;
        var candidate = new CleanupJsonCandidate
        {
            Path = verificationCondition.ExpectedPath,
            Kind = "final",
            Integrity = "verified",
            FileKind = "ordinary",
            WorkspaceAssociation = association,
            LeaseBoundary = leaseBoundary,
            Provenance = provenance,
            Verification = verificationCondition,
            Eligibility = "eligible",
            Action = "delete",
            Cause = null,
        };
        var planEntry = new CleanupJsonPlanEntry
        {
            Ordinal = 0,
            Path = candidate.Path,
            Kind = candidate.Kind,
            Integrity = candidate.Integrity,
            FileKind = candidate.FileKind,
            WorkspaceAssociation = association,
            LeaseBoundary = leaseBoundary,
            Provenance = provenance,
            Verification = verificationCondition,
            Eligibility = candidate.Eligibility,
            Action = candidate.Action,
            ResultEffect = new CleanupJsonEffectCondition
            {
                Outcome = "planned",
                Residual = "none",
            },
            Cause = null,
        };
        var catalogue = new CleanupJsonCatalogue
        {
            Coverage = "complete",
            Candidates = [candidate],
        };
        var result = new CleanupJsonResult
        {
            Mode = "dry-run",
            Catalogue = catalogue,
            Plan = new CleanupJsonPlan
            {
                Safety = "safe",
                Entries = [planEntry],
            },
            Preflight = new CleanupJsonPreflight
            {
                State = "complete",
                Cause = null,
            },
            Lease = new CleanupJsonLease
            {
                State = "not-requested",
                Cause = null,
            },
            Revalidation = new CleanupJsonCatalogueComparison
            {
                State = "not-requested",
                Planned = null,
                Observed = null,
                Cause = null,
            },
            Effects =
            [
                new CleanupJsonEffect
                {
                    Path = candidate.Path,
                    Kind = candidate.Kind,
                    Integrity = candidate.Integrity,
                    FileKind = candidate.FileKind,
                    WorkspaceAssociation = association,
                    LeaseBoundary = leaseBoundary,
                    Provenance = provenance,
                    Verification = verificationCondition,
                    Action = candidate.Action,
                    Outcome = "planned",
                    Residual = "none",
                    Cause = null,
                },
            ],
            Residuals =
            [
                new CleanupJsonResidual
                {
                    Path = candidate.Path,
                    Kind = candidate.Kind,
                    Integrity = candidate.Integrity,
                    FileKind = candidate.FileKind,
                    WorkspaceAssociation = association,
                    LeaseBoundary = leaseBoundary,
                    Provenance = provenance,
                    Verification = verificationCondition,
                    Action = candidate.Action,
                    Outcome = "verification-failed",
                    Residual = "retained",
                    Cause = "The synthetic Cleanup residual remains observable.",
                },
            ],
            Verification = new CleanupJsonVerification
            {
                State = "not-requested",
                Cause = null,
            },
            Findings =
            [
                new CleanupJsonFinding
                {
                    Code = "cleanup.invalid-input",
                    Status = "invalid",
                    Subject = null,
                    Cause = "The synthetic Cleanup finding is bounded.",
                },
            ],
        };

        return new CleanupJsonDocument
        {
            SchemaVersion = CleanupDefinitions.SchemaVersion,
            Command = CleanupDefinitions.CommandIdentity,
            Status = "invalid",
            Workspace = includeWorkspace
                ? new CleanupJsonWorkspace
                {
                    Path = workspacePath,
                    SelectedBy = "explicit-workspace",
                }
                : null,
            Result = result,
            Next = includeNext
                ? new CleanupJsonNext
                {
                    Command = CleanupDefinitions.CleanupCommandLine,
                    Reason = "Rerun the same Cleanup request.",
                }
                : null,
        };
    }

    private static string DefaultCandidatePath(
        RecoveryBundleCandidateKind kind,
        Guid operationId)
    {
        var extension = kind == RecoveryBundleCandidateKind.Final
            ? RecoveryBundleFormatV1.FinalFileExtension
            : RecoveryBundleFormatV1.DraftFileExtension;
        return Path.GetFullPath(
            Path.Combine(
                Path.GetTempPath(),
                "open-forge-cleanup-recovery",
                $"operation-{operationId:N}{extension}"));
    }

    private static CleanupWorkspaceAssociationState ReadAssociationState(
        CliWorkspace selected,
        CliWorkspace candidate)
        => PhysicalIdentityTracker.PathComparer.Equals(
            selected.PhysicalRoot,
            candidate.PhysicalRoot)
            ? CleanupWorkspaceAssociationState.CurrentWorkspace
            : CleanupWorkspaceAssociationState.Mismatched;

    private static CleanupLeaseBoundaryState ReadLeaseState(
        CleanupWorkspaceAssociationState associationState)
        => associationState == CleanupWorkspaceAssociationState.NotEstablished
            ? CleanupLeaseBoundaryState.NotEstablished
            : CleanupLeaseBoundaryState.Required;

    private static bool IsEligibleKindAndIntegrity(
        RecoveryBundleCandidateKind kind,
        RecoveryBundleIntegrity integrity)
        => (kind, integrity) is
            (RecoveryBundleCandidateKind.Final, RecoveryBundleIntegrity.Verified)
            or (RecoveryBundleCandidateKind.Draft, RecoveryBundleIntegrity.Incomplete);

    private static CleanupCandidateEligibility ReadEligibility(bool fullyEligible)
        => fullyEligible
            ? CleanupCandidateEligibility.Eligible
            : CleanupCandidateEligibility.Blocked;

    private static CleanupPlanAction ReadAction(CleanupCandidateEligibility eligibility)
    {
        if (eligibility == CleanupCandidateEligibility.Eligible)
        {
            return CleanupPlanAction.Delete;
        }

        if (eligibility == CleanupCandidateEligibility.Blocked)
        {
            return CleanupPlanAction.Preserve;
        }

        return CleanupPlanAction.NotEstablished;
    }
}
