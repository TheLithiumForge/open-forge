using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal sealed class ExtensionInspectComparisonBuilder(MarkdownFingerprintReader fingerprintReader)
{
    private readonly ExtensionInspectFingerprintBuilder _fingerprintBuilder = new(fingerprintReader);

    internal ExtensionInspectComparisonFacts Build(ExtensionInspectComparisonBuildInput input)
    {
        var baselineRecords = ExtensionInspectFingerprintBuilder.ReadBaselineRecords(
            input.Lifecycle,
            input.InstalledPackage);
        var baselineFingerprints = ExtensionInspectFingerprintBuilder.ReadBaselineFingerprints(baselineRecords);
        var currentMarkdownFacts = new Dictionary<string, MarkdownFingerprintFacts>(StringComparer.Ordinal);
        var intendedMarkdownFacts = new Dictionary<string, MarkdownFingerprintFacts>(StringComparer.Ordinal);
        var currentFingerprints = _fingerprintBuilder.ReadCurrentFingerprints(
            input.PathFacts.Current,
            input.Findings,
            currentMarkdownFacts);
        var intendedFingerprints = _fingerprintBuilder.ReadIntendedFingerprints(
            input.SourcePathProjections,
            input.Findings,
            intendedMarkdownFacts);
        var generated = ExtensionInspectGeneratedBuilder.Build(
            currentMarkdownFacts,
            intendedMarkdownFacts,
            input.PathFacts.State,
            input.Findings);
        var comparison = ExtensionInspectComparisonProjector.Build(
            new ExtensionInspectComparisonInput
            {
                Lifecycle = input.Lifecycle,
                InstalledPackage = input.InstalledPackage,
                AvailablePackage = input.AvailablePackage,
                AvailableClosure = input.AvailableClosure,
                Dependencies = input.Dependencies,
                BaselineRecords = baselineRecords,
                Baseline = baselineFingerprints,
                Current = currentFingerprints,
                Intended = intendedFingerprints,
                Findings = input.Findings,
            });

        return new ExtensionInspectComparisonFacts
        {
            Generated = generated,
            Comparison = comparison,
        };
    }
}
