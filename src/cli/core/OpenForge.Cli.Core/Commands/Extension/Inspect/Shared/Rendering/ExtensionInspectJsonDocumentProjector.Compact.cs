using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Presentation;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;

internal static partial class ExtensionInspectJsonDocumentProjector
{
    internal static ExtensionInspectCompactJsonResult CreateCompact(ExtensionInspectResult result)
        => new()
        {
            Subject = Subject(result.Subject),
            Source = Source(result.Source),
            Lifecycle = Lifecycle(result.Lifecycle),
            Installed = Installed(result.Installed),
            Available = Available(result.Available),
            Dependencies = Dependencies(result.Dependencies),
            PathFacts = PathFacts(result.PathFacts),
            Comparison = CompactComparison(result.Comparison),
            Generated = Generated(result.Generated),
            Findings = result.Findings.Select(Finding).ToArray(),
            Counts = Counts(result.Counts),
        };

    private static ExtensionInspectCompactJsonComparison CompactComparison(ExtensionInspectComparison value)
        => new()
        {
            State = ExtensionInspectWireVocabulary.ComparisonState(value.State),
            Mode = ExtensionInspectWireVocabulary.ComparisonMode(value.Mode),
            Baseline = CompactSide(value.Baseline),
            Current = CompactSide(value.Current),
            Intended = CompactSide(value.Intended),
            Paths = value.Paths.Select(CompactPathComparison).ToArray(),
            Dependencies = DependencyComparison(value.Dependencies),
        };

    private static ExtensionInspectCompactJsonComparisonSide CompactSide(ExtensionInspectComparisonSide value)
        => new()
        {
            State = ExtensionInspectWireVocabulary.ComparisonSideState(value.State),
        };

    private static ExtensionInspectCompactJsonPathComparison CompactPathComparison(ExtensionInspectPathComparison value)
        => new()
        {
            Path = value.Path,
            Relation = ExtensionInspectWireVocabulary.PathRelation(value.Relation),
            BaselineOwners = value.BaselineOwners.ToArray(),
            CurrentOwners = value.CurrentOwners.ToArray(),
            IntendedOwners = value.IntendedOwners.ToArray(),
        };
}
