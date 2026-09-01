using OpenForge.Cli.Core.Commands.References.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.References.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.References.Shared.Rendering;

public sealed class ReferencesHumanRenderingTests
{
    [Fact(DisplayName = "References compact rendering preserves requested sections, coverage, counts, Level 1 rows, findings, and next action"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void CompactRenderingPreservesDirectEvidence()
    {
        var output = ReferencesHumanRenderer.Render(
            ReferencesPresentationTestData.Presentation(
                ReferencesPresentationTestData.AttentionResult(),
                CliOutputFormat.Human,
                CliView.Compact));

        Assert.Contains("result=attention", output, StringComparison.Ordinal);
        Assert.Contains("Outgoing", output, StringComparison.Ordinal);
        Assert.Contains("coverage=complete", output, StringComparison.Ordinal);
        Assert.Contains("occurrences=2", output, StringComparison.Ordinal);
        Assert.Contains("Level 1", output, StringComparison.Ordinal);
        Assert.Contains("mailto:docs@example.invalid", output, StringComparison.Ordinal);
        Assert.Contains("references.destination-unsupported", output, StringComparison.Ordinal);
        Assert.Contains("Next:", output, StringComparison.Ordinal);
        Assert.DoesNotContain("Incoming", output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "References expanded rendering exposes workspace, source, direction, filters, layers, locations, destinations, resolution, and provenance"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void ExpandedRenderingPreservesExplanatoryEvidence()
    {
        var output = ReferencesHumanRenderer.Render(
            ReferencesPresentationTestData.Presentation(
                ReferencesPresentationTestData.CompleteResult(),
                CliOutputFormat.Human,
                CliView.Expanded));

        Assert.Contains("Workspace:", output, StringComparison.Ordinal);
        Assert.Contains("Source:", output, StringComparison.Ordinal);
        Assert.Contains("Direction: both", output, StringComparison.Ordinal);
        Assert.Contains("include", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("layer: base", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("layer: overwrite", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("location:", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("raw destination:", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("resolution: complete", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("provenance:", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Incoming", output, StringComparison.Ordinal);
        Assert.Contains("Outgoing", output, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "References human views keep deterministic status framing and escape control-bearing authored values"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    [InlineData((int)CliView.Compact)]
    [InlineData((int)CliView.Expanded)]
    public void HumanViewsEscapeAuthoredControlValues(int viewValue)
    {
        var view = (CliView)viewValue;
        var output = ReferencesHumanRenderer.Render(
            ReferencesPresentationTestData.Presentation(
                ReferencesPresentationTestData.ControlCharacterResult(),
                CliOutputFormat.Human,
                view));

        Assert.Contains("line\\nbreak", output, StringComparison.Ordinal);
        Assert.Contains("control\\tcause", output, StringComparison.Ordinal);
        Assert.DoesNotContain("line\nbreak", output, StringComparison.Ordinal);
        Assert.DoesNotContain("control\tcause", output, StringComparison.Ordinal);
    }
}
