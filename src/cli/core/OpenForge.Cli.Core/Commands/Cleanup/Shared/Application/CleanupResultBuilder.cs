using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Cleanup.Shared.Application;

internal sealed class CleanupResultBuilder(CleanupPlan plan)
{
    internal CleanupPlan Plan { get; } = plan;

    internal CleanupPreflight Preflight { get; set; } = new() { State = CleanupPreflightState.NotRequested };

    internal CleanupLease Lease { get; set; } = new() { State = CleanupLeaseState.NotRequested };

    internal CleanupCatalogueComparison Revalidation { get; set; } = new() { State = CleanupCatalogueComparisonState.NotRequested };

    internal ImmutableArray<CleanupEffect> Effects { get; set; } = [];

    internal CleanupVerification Verification { get; set; } = new() { State = CleanupVerificationState.NotRequested };

    internal ImmutableArray<CleanupFinding> Findings { get; set; } = [];

    internal CleanupResult Build()
    {
        var status = ReadStatus();
        return new CleanupResult
        {
            Status = status,
            Workspace = Plan.Request?.Workspace,
            Next = CleanupDefinitions.ReadNextAction(status),
            Facts = CleanupResultFacts.Create(
                Plan,
                Preflight,
                Lease,
                Revalidation,
                Effects,
                [.. Effects.Where(effect => effect.Residual != CleanupEffectResidual.None).Select(effect => CleanupResidual.Create(effect, effect.Cause))],
                Verification,
                Findings),
        };
    }

    internal void PreserveUnattempted(RecoveryBundleCatalogueResult? observed)
        => Effects = [.. Plan.Entries.Where(entry => entry.Action == CleanupPlanAction.Delete).Select(entry => UnattemptedEffect(entry, observed))];

    internal void AddFinding(CleanupFindingCode code, string cause, string? subject = null)
        => Findings = Findings.Add(CleanupFinding.Create(code, cause, subject));

    private static CleanupEffect UnattemptedEffect(CleanupPlanEntry entry, RecoveryBundleCatalogueResult? observed)
    {
        var retained = observed is { State: RecoveryBundleCatalogueState.Available }
            && observed.Candidates.Any(candidate => PhysicalIdentityTracker.PathComparer.Equals(candidate.Path, entry.Path)
                && candidate.Kind == entry.Kind
                && candidate.Integrity is RecoveryBundleIntegrity.Verified or RecoveryBundleIntegrity.Malformed
                    or RecoveryBundleIntegrity.Unsupported or RecoveryBundleIntegrity.Incomplete);
        if (retained)
        {
            return CleanupEffect.Create(entry, CleanupEffectOutcome.NotStarted, CleanupEffectResidual.Retained);
        }

        return CleanupEffect.Create(entry, CleanupEffectOutcome.CompletionUnknown, CleanupEffectResidual.Unknown,
            "No deletion was attempted; the candidate's current disposition was not established.");
    }

    private CliSemanticStatus ReadStatus()
    {
        if (Findings.Any(finding => finding.Status == CliSemanticStatus.Failed))
        {
            return CliSemanticStatus.Failed;
        }

        if (Findings.Any(finding => finding.Status == CliSemanticStatus.Blocked))
        {
            return CliSemanticStatus.Blocked;
        }

        if (Findings.Any(finding => finding.Status == CliSemanticStatus.Interrupted))
        {
            return CliSemanticStatus.Interrupted;
        }

        if (Findings.Any(finding => finding.Status == CliSemanticStatus.Invalid))
        {
            return CliSemanticStatus.Invalid;
        }

        if (Findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete))
        {
            return CliSemanticStatus.Incomplete;
        }

        return CliSemanticStatus.Complete;
    }
}
