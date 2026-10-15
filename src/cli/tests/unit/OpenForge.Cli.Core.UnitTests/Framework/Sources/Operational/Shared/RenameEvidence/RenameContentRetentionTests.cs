using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational.Shared.RenameEvidence;

public sealed class RenameContentRetentionTests
{
    [Theory(DisplayName = "Rename content reuses only a parse of the exact current bytes"),
        InlineData(false), InlineData(true), Trait("Feature", "repair-rename"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
    public void ExactCurrentContentOwnsRetainedParse(bool changed)
    {
        const string original = "# Original\n\nAuthored text.\n";
        var parser = new MarkdownDocumentParser();
        var retained = parser.Parse(original);
        var bytes = Encoding.UTF8.GetBytes(changed ? "# Changed\n\nAuthored text.\n" : original);
        var content = RenameContentReader.Read(bytes, retained);
        var fresh = RenameContentReader.Read(bytes);

        if (changed) Assert.NotSame(retained, content.Document);
        else Assert.Same(retained, content.Document);
        Assert.Equal(fresh.Hash, content.Hash);
        Assert.Equal(fresh.NormalizedHash, content.NormalizedHash);
        Assert.Equal(fresh.Lines, content.Lines);
        Assert.Equal(fresh.HasAuthoredContent, content.HasAuthoredContent);
    }
}
