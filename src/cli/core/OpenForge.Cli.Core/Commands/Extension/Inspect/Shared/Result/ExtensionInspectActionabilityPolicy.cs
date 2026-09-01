using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectActionabilityPolicy
{
    internal static bool IsActionable(ExtensionInspectActionabilityInput input)
        => input.Comparison.Mode == ExtensionInspectComparisonMode.ThreeWay
            && input.Comparison.State == ExtensionInspectComparisonState.Complete
            && input.Generated.State == ExtensionInspectGeneratedState.Complete
            && input.Lifecycle.Trust == LifecycleExtensionTrust.Trusted
            && input.Source.State == ExtensionSourceReadState.Complete
            && input.Comparison.Baseline.State == ExtensionInspectComparisonSideState.Available
            && input.Comparison.Current.State == ExtensionInspectComparisonSideState.Available
            && input.Comparison.Intended.State == ExtensionInspectComparisonSideState.Available
            && input.Comparison.Baseline.Fingerprints
                .Concat(input.Comparison.Current.Fingerprints)
                .Concat(input.Comparison.Intended.Fingerprints)
                .All(fact => fact.Fingerprint is
                {
                    Kind: ExtensionInspectFingerprintKind.Semantic,
                    Policy: ExtensionInspectDefinitions.FingerprintPolicy,
                })
            && (input.Comparison.Paths.Any(path => path.Relation == ExtensionInspectPathRelation.Changed)
                || input.Comparison.Dependencies.Relation == ExtensionInspectDependencyRelation.Changed)
            && !input.Findings.Any(finding => finding.Code is ExtensionInspectFindingCode.PathCurrentDiverged
                or ExtensionInspectFindingCode.PathMissing
                or ExtensionInspectFindingCode.PathRetired
                or ExtensionInspectFindingCode.OwnershipConflict
                or ExtensionInspectFindingCode.DependencyConflict
                or ExtensionInspectFindingCode.FingerprintFallback
                or ExtensionInspectFindingCode.GeneratedBoundaryInvalid);
}
