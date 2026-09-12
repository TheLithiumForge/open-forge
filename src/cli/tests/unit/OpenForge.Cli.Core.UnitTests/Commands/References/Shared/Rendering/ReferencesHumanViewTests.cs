using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.References.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.References.Shared.Rendering;

public sealed class ReferencesHumanViewTests
{
    [Theory(DisplayName = "References human views retain occurrence order and network uncertainty without changing JSON"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    [InlineData((int)CliView.Compact)]
    [InlineData((int)CliView.Expanded)]
    public void ViewsPreserveDirectFacts(int value)
    {
        var result = ReferencesPresentationTestData.CompleteResult();
        var jsonRequest = ReferencesPresentationTestData.Presentation(result, CliOutputFormat.Json);
        var before = ReferencesJsonRenderer.Render(jsonRequest);
        var output = ReferencesHumanRenderer.Render(ReferencesPresentationTestData.Presentation(result, CliOutputFormat.Human, (CliView)value));

        Assert.Contains(".agents/alpha.md:7:3", output, StringComparison.Ordinal);
        Assert.Contains(".agents/docs.md:4:1", output, StringComparison.Ordinal);
        Assert.Contains(".agents/docs.overwrite.md:6:1", output, StringComparison.Ordinal);
        Assert.True(output.IndexOf(".agents/docs.md:4:1", StringComparison.Ordinal) < output.IndexOf(".agents/docs.overwrite.md:6:1", StringComparison.Ordinal));
        Assert.Contains("guide.md#Start", output, StringComparison.Ordinal);
        Assert.Contains("external URL; not checked over the network", output, StringComparison.Ordinal);
        Assert.DoesNotContain("bytes ", output, StringComparison.Ordinal);
        Assert.Equal(before, ReferencesJsonRenderer.Render(jsonRequest));
        Assert.DoesNotContain("Found by:", ReferencesHumanRenderer.Render(ReferencesPresentationTestData.Presentation(result, CliOutputFormat.Human, CliView.Compact)), StringComparison.Ordinal);
    }

    [Theory(DisplayName = "References incomplete empty sections and findings remain visible before occurrences"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    [InlineData((int)CliView.Compact)]
    [InlineData((int)CliView.Expanded)]
    public void ViewsKeepMissingCoverage(int value)
    {
        var blocked = ReferencesHumanRenderer.Render(ReferencesPresentationTestData.Presentation(ReferencesPresentationTestData.BlockedResult(), CliOutputFormat.Human, (CliView)value));
        Assert.Contains("inspection is not complete", blocked, StringComparison.Ordinal);
        Assert.DoesNotContain("No direct links found", blocked, StringComparison.Ordinal);
        var attention = ReferencesHumanRenderer.Render(ReferencesPresentationTestData.Presentation(ReferencesPresentationTestData.AttentionResult(), CliOutputFormat.Human, (CliView)value));
        Assert.True(attention.IndexOf("references.destination-unsupported", StringComparison.Ordinal) < attention.IndexOf("Outgoing links:", StringComparison.Ordinal));
        Assert.Contains(".agents/docs.md:3:1", attention, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "References resolution labels preserve every typed distinction"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    [InlineData((int)ReferencesTargetResolution.Complete, "target found")]
    [InlineData((int)ReferencesTargetResolution.Missing, "target missing")]
    [InlineData((int)ReferencesTargetResolution.FragmentMissing, "target fragment missing")]
    [InlineData((int)ReferencesTargetResolution.Malformed, "malformed destination")]
    [InlineData((int)ReferencesTargetResolution.Absolute, "absolute destination is not supported")]
    [InlineData((int)ReferencesTargetResolution.Query, "query in destination is not supported")]
    [InlineData((int)ReferencesTargetResolution.EncodingUnsupported, "destination encoding is not supported")]
    [InlineData((int)ReferencesTargetResolution.OutsideWorkspace, "target is outside the workspace")]
    [InlineData((int)ReferencesTargetResolution.PhysicalEscape, "target crosses the workspace boundary")]
    [InlineData((int)ReferencesTargetResolution.Ambiguous, "target is ambiguous; no choice made")]
    [InlineData((int)ReferencesTargetResolution.Unreadable, "target could not be read")]
    [InlineData((int)ReferencesTargetResolution.Unsupported, "destination is not supported")]
    [InlineData((int)ReferencesTargetResolution.ExternalUnchecked, "external URL; not checked over the network")]
    public void ResolutionLabels(int value, string expected)
        => Assert.Equal(expected, ReferencesHumanValues.Resolution((ReferencesTargetResolution)value));

    [Fact(DisplayName = "References rejects an undefined human resolution"), Trait("Feature", "references"), Trait("Evidence", "Unit")]
    public void UndefinedResolution()
        => Assert.Throws<ArgumentOutOfRangeException>(() => ReferencesHumanValues.Resolution((ReferencesTargetResolution)int.MaxValue));
}
