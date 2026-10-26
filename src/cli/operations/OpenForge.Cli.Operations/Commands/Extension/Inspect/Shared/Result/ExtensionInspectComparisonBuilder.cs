using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Settings;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal sealed class ExtensionInspectComparisonBuilder(MarkdownFingerprintReader fingerprintReader)
{
    private readonly ExtensionInspectFingerprintBuilder _fingerprintBuilder = new(fingerprintReader);

    internal ExtensionInspectComparisonFacts Build(ExtensionInspectComparisonBuildInput input)
    {
        var currentMarkdownFacts = new Dictionary<string, MarkdownFingerprintFacts>(StringComparer.Ordinal);
        var intendedMarkdownFacts = new Dictionary<string, MarkdownFingerprintFacts>(StringComparer.Ordinal);
        var currentFingerprints = input.Layout is not { } layout ? [] : _fingerprintBuilder.ReadCurrentFingerprints(
            layout,
            input.PathFacts.Current,
            input.Findings,
            currentMarkdownFacts);
        IReadOnlyList<ExtensionInspectFingerprintFact> intendedFingerprints = [];
        if (input.Layout is { } intendedLayout && input.Frontmatter is { } form)
        {
            intendedFingerprints = _fingerprintBuilder.ReadIntendedFingerprints(
                intendedLayout,
                input.SourcePathProjections,
                form,
                input.Findings,
                intendedMarkdownFacts);
        }
        else if (input.AvailablePackage is not null || input.InstalledPackage is not null)
        {
            ExtensionInspectFindingPolicy.Add(input.Findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.FingerprintUnavailable,
                Path = WorkspaceSettingsDefinitions.RelativePath,
                Cause = "The workspace frontmatter form is unavailable.",
            });
        }
        var generated = ExtensionInspectGeneratedBuilder.Build(
            currentMarkdownFacts,
            intendedMarkdownFacts,
            input.PathFacts.State,
            input.Findings);
        var comparison = ExtensionInspectComparisonProjector.Build(
            new ExtensionInspectComparisonInput
            {
                Layout = input.Layout,
                Ownership = input.Ownership,
                Frontmatter = input.Frontmatter,
                InstalledPackage = input.InstalledPackage,
                AvailablePackage = input.AvailablePackage,
                AvailableClosure = input.AvailableClosure,
                Dependencies = input.Dependencies,
                Current = currentFingerprints,
                CurrentPaths = input.PathFacts.Current,
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
