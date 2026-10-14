using System.Text;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Content;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational.Shared.RenameEvidence;

[Trait("Feature", "rename-evidence"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class RenameContentComparerTests
{
    [Theory(DisplayName = "Weighted identical lines produce the exact 89 90 and 100 threshold scores")]
    [InlineData(89), InlineData(90), InlineData(100)]
    public void WeightedThreshold(int unchanged)
    {
        var old = Read(string.Concat(Enumerable.Range(0, 100).Select(index => $"old-{index:000}\n")));
        var current = Read(string.Concat(Enumerable.Range(0, 100).Select(index => index < unchanged ? $"old-{index:000}\n" : $"new-{index:000}\n")));

        var comparison = Compare(old, current);

        Assert.Equal(unchanged, comparison.Similarity);
        Assert.Equal(unchanged == 100, comparison.SameContent);
        Assert.Equal(unchanged >= 90, comparison.IsStrong);
    }

    [Fact(DisplayName = "Exact hashes preserve BOM line endings and the final unterminated line")]
    public void ExactHashes()
    {
        var content = Read("abc");
        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", content.Hash);
        Assert.False(Compare(Read("\uFEFFabc\r\n"), Read("abc\n")).SameContent);
        Assert.Equal(100, Compare(Read("one\r\ntwo"), Read("one\r\ntwo")).Similarity);
    }

    [Theory(DisplayName = "Empty bodies metadata and generated Entries alone cannot establish content identity")]
    [InlineData("")]
    [InlineData("\uFEFF")]
    [InlineData("\uFEFF \r\n\t")]
    [InlineData(" \r\n\t")]
    [InlineData("---\ndescription: Recorded topic\ntags: [Guidance]\n---\n \n")]
    [InlineData("## Entries\n\n- [Recorded topic](note.md) - #Guidance\n")]
    public void EmptyAuthoredBody(string text)
    {
        var content = Read(text);
        Assert.False(content.HasAuthoredContent);
        Assert.False(Compare(content, content).IsStrong);
    }

    [Fact(DisplayName = "Scored comparison requires an identical non-whitespace authored line")]
    public void MetadataAndWhitespaceAreInsufficient()
    {
        var prefix = "---\ndescription: Recorded topic\ntags: [Guidance]\n---\n" + new string(' ', 1000) + "\n";
        var comparison = Compare(Read(prefix + "first\n"), Read(prefix + "other\n"));
        Assert.True(comparison.Similarity >= 90);
        Assert.False(comparison.IsStrong);
    }

    [Fact(DisplayName = "A byte order mark does not turn a whitespace line into authored evidence")]
    public void BomWhitespaceLine()
    {
        var prefix = "\uFEFF" + new string(' ', 1000) + "\n";
        var comparison = Compare(Read(prefix + "first\n"), Read(prefix + "other\n"));
        Assert.True(comparison.Similarity >= 90);
        Assert.False(comparison.IsStrong);
    }

    [Fact(DisplayName = "The weighted subsequence preserves order and duplicate-line byte weights")]
    public void OrderAndWeights()
    {
        var comparison = Compare(Read("a\nlonger\na\n"), Read("longer\na\na\n"));
        Assert.Equal(81, comparison.Similarity);
        Assert.False(comparison.SameContent);
        Assert.Equal(0, Compare(Read("first\n"), Read("other\n")).Similarity);
    }

    [Theory(DisplayName = "Beyond either comparison bound exact hashing remains available without a guessed score")]
    [InlineData(true), InlineData(false)]
    public void Bounds(bool bytes)
    {
        var text = bytes ? new string('x', 1024 * 1024 + 1) : string.Concat(Enumerable.Repeat("x\n", 2001));
        var content = Read(text);
        var comparison = Compare(content, content);
        Assert.Null(comparison.Similarity);
        Assert.True(comparison.SameContent);
        Assert.True(content.HasAuthoredContent);
    }

    private static RenameContent Read(string text)
        => RenameContentReader.Read(Encoding.UTF8.GetBytes(text));

    [Fact(DisplayName = "Authored prose within Entries remains authored content outside its generated list")]
    public void AuthoredEntriesProse()
    {
        var content = Read("## Entries\n\nAuthored notes about this list.\n\n- [Recorded topic](note.md) - #Guidance\n");
        Assert.True(content.HasAuthoredContent);
        Assert.True(Compare(content, content).HasCommonAuthoredLine);
    }

    private static RenameContentComparison Compare(RenameContent old, RenameContent current)
        => RenameContentComparer.Compare(old, current, TestContext.Current.CancellationToken);
}
