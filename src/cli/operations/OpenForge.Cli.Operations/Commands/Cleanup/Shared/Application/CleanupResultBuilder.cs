using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.Serialization;
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

    internal static CleanupResult BuildStores(CleanupRequest request, IReadOnlyList<CleanupStoreApplication> stores)
    {
        var plan = CleanupPlan.Aggregate(request, [.. stores.Select(store => store.Result.Plan)]);
        var results = stores.Select(store => store.Result.Build()).ToArray();
        var effects = results.SelectMany(result => result.Effects).ToDictionary(effect => effect.Path, StringComparer.Ordinal);
        var aggregate = new CleanupResultBuilder(plan)
        {
            Findings = [.. results.SelectMany(result => result.Findings)],
            Preflight = results.Select(result => result.Preflight).OrderByDescending(value => value.State).First(),
            Lease = results.Select(result => result.Lease).OrderByDescending(value => value.State).First(),
            Revalidation = AggregateComparison(plan, results),
            Verification = results.Select(result => result.Verification).OrderByDescending(value => value.State).First(),
            Effects = [.. plan.Entries.Where(entry => effects.ContainsKey(entry.Path)).Select(entry =>
            {
                var effect = effects[entry.Path];
                return CleanupEffect.Create(entry, effect.Outcome, effect.Residual, effect.Cause);
            })],
        };
        return aggregate.Build() with
        {
            Stores = stores.Select((store, index) => new CleanupStoreResult(
                RecoveryWireVocabulary.StoreKind(store.Observation.Kind), store.Observation.Location?.WorkspaceDirectory,
                store.Result.Revalidation.Cause ?? store.Observation.Catalogue.Cause ?? store.Result.Verification.Cause, results[index].Facts)).ToArray(),
        };
    }

    private static CleanupCatalogueComparison AggregateComparison(CleanupPlan plan, IReadOnlyList<CleanupResult> results)
    {
        var comparison = results.Select(result => result.Revalidation).OrderByDescending(value => value.State).First();
        if (comparison.State == CleanupCatalogueComparisonState.NotRequested)
        {
            return comparison;
        }

        if (comparison.State == CleanupCatalogueComparisonState.Matched && plan.Safety != CleanupPlanSafety.Safe)
        {
            comparison = comparison with
            {
                State = plan.Catalogue.IsComplete
                    ? CleanupCatalogueComparisonState.Blocked : CleanupCatalogueComparisonState.Incomplete,
                Cause = results.SelectMany(result => result.Findings).FirstOrDefault()?.Cause,
            };
        }

        var observed = results.Where(result => !result.Plan.DeletionEntries.IsEmpty)
            .Select(result => result.Revalidation.Observed).ToArray();
        return comparison with
        {
            Planned = plan.Catalogue,
            Observed = observed.All(catalogue => catalogue is not null)
                ? CleanupCatalogue.Create(plan.Catalogue.Coverage,
                    [.. observed.SelectMany(catalogue => catalogue?.Candidates ?? []).OrderBy(candidate => candidate.Path, StringComparer.Ordinal)])
                : null,
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
            "No deletion was attempted. The candidate's current disposition was not established.");
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

        if (Findings.Any(finding => finding.Status == CliSemanticStatus.Attention))
        {
            return CliSemanticStatus.Attention;
        }

        return CliSemanticStatus.Complete;
    }
}
