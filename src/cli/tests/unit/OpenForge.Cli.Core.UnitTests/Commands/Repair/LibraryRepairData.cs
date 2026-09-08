using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

internal static class LibraryRepairData
{
    internal static LibraryResidualEvidence Evidence(
        RecoveryEntryKind kind = RecoveryEntryKind.RelativeFileLinkCreate,
        RecoveryBundleTargetComparisonState comparison = RecoveryBundleTargetComparisonState.Intended)
    {
        var workspace = LibraryMutationPlanningData.Workspace;
        var ordinary = RecoveryEntryState.Ordinary(RecoveryContentIdentity.FromBytes(LibraryMutationPlanningData.RecordBytes()));
        var intendedOrdinary = RecoveryEntryState.Ordinary(RecoveryContentIdentity.FromBytes(LibraryMutationPlanningData.RecordBytes(LibraryMutationPlanningData.Leaf)));
        var link = RecoveryEntryState.RelativeLink(RelativeFileLinkIdentity.Create(
            NoFollowLinkKind.SymbolicLink, "../../shared/team-knowledge/.agents/directives/review.md"));
        if (kind == RecoveryEntryKind.OrdinaryReplaceGeneratedRegion)
        {
            const string host = "# Directives\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n";
            const string end = "<!-- open-forge:generated-index:end -->\n";
            ordinary = RecoveryEntryState.Ordinary(RecoveryContentIdentity.FromBytes(Encoding.UTF8.GetBytes(host + end)));
            intendedOrdinary = RecoveryEntryState.Ordinary(RecoveryContentIdentity.FromBytes(
                Encoding.UTF8.GetBytes(host + "- [Review](review.md) - #Directive\n" + end)));
        }

        var prior = kind switch
        {
            RecoveryEntryKind.OrdinaryCreate or RecoveryEntryKind.RelativeFileLinkCreate => RecoveryEntryState.Missing,
            RecoveryEntryKind.RelativeFileLinkDelete => link,
            _ => ordinary,
        };
        var intended = kind switch
        {
            RecoveryEntryKind.RelativeFileLinkCreate => link,
            RecoveryEntryKind.RelativeFileLinkDelete or RecoveryEntryKind.OrdinaryDelete => RecoveryEntryState.Missing,
            _ => intendedOrdinary,
        };
        var target = kind is RecoveryEntryKind.RelativeFileLinkCreate or RecoveryEntryKind.RelativeFileLinkDelete
            ? ".agents/directives/review.md"
            : ".agents/open-forge.libraries.json";
        if (kind == RecoveryEntryKind.OrdinaryReplaceGeneratedRegion)
        {
            target = ".agents/directives/_directives.md";
        }

        var entry = RecoveryEntry.Create(0, CanonicalRelativePath.Create(target), kind, prior, intended,
            prior.Kind == RecoveryEntryStateKind.OrdinaryFile ? "payloads/00000000.bin" : null);
        var verified = new RecoveryBundleVerifiedRead
        {
            BundlePath = Path.Combine(workspace.PhysicalRoot, "observed-residual.zip"),
            WorkspacePhysicalPath = WorkspaceIdentity.NormalizePhysicalPath(workspace.PhysicalRoot),
            WorkspaceKey = WorkspaceIdentity.Key(workspace.PhysicalRoot),
            Command = "library sync",
            Attribution = RecoveryBundleAttribution.Create(RecoveryBundleProducer.Library, RecoveryBundleOperation.Sync, workspace),
            OperationId = new Guid("97c67153-7cdd-4f51-9578-3182d1673b8d"),
            Entries = [entry],
        };
        var context = new RecoveryEntryComparisonContext(workspace, entry);
        var observed = comparison == RecoveryBundleTargetComparisonState.Prior ? prior : intended;
        if (comparison == RecoveryBundleTargetComparisonState.Third)
        {
            observed = RecoveryEntryState.RelativeLink(RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../../shared/changed.md"));
        }

        var leaf = observed.Kind switch
        {
            RecoveryEntryStateKind.Missing => NoFollowLeafObservation.Missing(context.LogicalPath),
            RecoveryEntryStateKind.RelativeFileLink => NoFollowLeafObservation.CreateRelativeFileLink(context.LogicalPath, observed.RelativeFileLink!),
            _ => NoFollowLeafObservation.OrdinaryFile(context.LogicalPath),
        };
        if (comparison is RecoveryBundleTargetComparisonState.Blocked or RecoveryBundleTargetComparisonState.Unavailable)
        {
            leaf = NoFollowLeafObservation.Classified(context.LogicalPath,
                comparison == RecoveryBundleTargetComparisonState.Blocked ? NoFollowLeafState.ReparsePoint : NoFollowLeafState.Inaccessible,
                comparison == RecoveryBundleTargetComparisonState.Unavailable
                    ? new FilesystemFailure(FilesystemFailureKind.AccessDenied, "Independent selected target observation was inaccessible.")
                    : null);
        }

        var content = leaf.State == NoFollowLeafState.OrdinaryFile
            ? new RecoveryOrdinaryContentObservation(context.LogicalPath, observed.OrdinaryFile, failure: null)
            : null;
        var current = new RecoveryEntryComparison
        {
            Input = new RecoveryEntryComparisonInput(context, leaf, content),
            State = comparison,
            Observed = comparison is RecoveryBundleTargetComparisonState.Blocked or RecoveryBundleTargetComparisonState.Unavailable ? null : observed,
            Cause = comparison is RecoveryBundleTargetComparisonState.Third or RecoveryBundleTargetComparisonState.Blocked
                or RecoveryBundleTargetComparisonState.Unavailable ? "Independent selected boundary refusal." : null,
        };
        var residual = new RecoveryEntrySetObservation(workspace, RecoveryBundleCandidateSnapshot.VerifiedFinal(verified), [current]);
        return new LibraryResidualEvidence(LibraryId.Create("team-knowledge"),
            LibraryMutationPlanningData.Record(LibraryMutationPlanningData.Leaf), verifiedPriorRecord: null, residual, current);
    }

    internal static RepairLibraryPlanningInput Input(LibraryResidualEvidence evidence, bool automatic = true)
        => new()
        {
            Request = new RepairRequest(LibraryMutationPlanningData.Workspace, RepairMode.Apply, automatic, [], allowInteraction: false),
            References = [],
            Libraries = [new RepairLibraryRecoveryProposal(evidence)],
            WizardRelinks = [],
            WizardLibraries = null,
        };

    internal static RepairPlan Plan(LibraryResidualEvidence? evidence = null)
    {
        var proposal = new RepairLibraryRecoveryProposal(evidence ?? Evidence());
        var selected = new RepairSelectedLibraryRecovery(proposal, [RepairSelectionOrigin.Automatic]);
        var step = new RepairLibraryRecoveryStep
        {
            Ordinal = 0,
            Selection = selected,
            Dependency = new RepairDependency([RepairDependencyDomain.WorkspaceContainment, RepairDependencyDomain.LibraryRecord, RepairDependencyDomain.LibraryResidual]),
            Verification = new RepairVerificationRequirement([RepairVerificationKind.NoFollowIdentity, RepairVerificationKind.PriorState]),
            Effect = new RepairLibraryRecoveryEffect(selected),
            Outcome = RepairStepOutcome.Planned,
        };
        return new RepairPlan(Input(proposal.Evidence).Request,
            new RepairSelection(RepairSelectionMode.Automatic, [], [], new RepairLibrarySelection([selected], [])), [], [], [step]);
    }

    internal static RepairLibraryExecution Execution()
        => new()
        {
            ReferenceReceipts = [],
            LibraryReceipts = [],
            ForwardPreparation = null,
            ForwardCleanup = null,
            PostDiagnosis = null,
            Cancellation = null,
            UnexpectedFailure = null,
        };
}
