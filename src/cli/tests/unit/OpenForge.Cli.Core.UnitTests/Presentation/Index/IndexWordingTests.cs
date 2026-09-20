using OpenForge.Cli.Core.Presentation.Index.Shared.Wording;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Index;

public sealed class IndexWordingTests
{
    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void OneFileAndSectionUseSingularGrammar()
    {
        Assert.Equal(new[]
        {
            "The Entries section is current in 1 file. Nothing to do.",
            "Updated the Entries section in 1 of 1 file.",
            "Would update the Entries section in 1 of 1 file.",
            "Index stopped after 1 of 1 file.",
            "Index was cancelled. Stopped after 1 of 1 file.",
            "Nothing was written. The other 1 section is current.",
            "1 file is current.",
        }, RenderCounts(1));
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void PluralCatalogueSentencesRemainExact()
    {
        Assert.Equal(new[]
        {
            "Entries sections are current in all 2 files. Nothing to do.",
            "Updated the Entries section in 1 of 2 files.",
            "Would update the Entries section in 1 of 2 files.",
            "Index stopped after 1 of 2 files.",
            "Index was cancelled. Stopped after 1 of 2 files.",
            "Nothing was written. The other 2 sections are current.",
            "2 files are current.",
        }, RenderCounts(2));
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    public void ZeroRetainsPluralGrammar()
    {
        Assert.Equal("Entries sections are current in all 0 files. Nothing to do.", IndexWording.Current(0));
        Assert.Equal("0 files are current.", IndexWording.Unchanged(0));
        Assert.Equal("Nothing was written. The other 0 sections are current.", IndexWording.OtherCurrent(0));
    }

    private static string[] RenderCounts(long files) =>
    [
        IndexWording.Current(files), IndexWording.Updated(1, files), IndexWording.Preview(1, files),
        IndexWording.Failed(1, files), IndexWording.CancelledAfter(1, files),
        IndexWording.OtherCurrent(files), IndexWording.Unchanged(files),
    ];
}
