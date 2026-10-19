using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational;

[Trait("Feature", "reference-encoding"), Trait("Evidence", "Unit")]
public sealed class LocalReferenceCanonicalEncodingTests
{
    [Theory(DisplayName = "Canonical destination paths retain required encoding and literal percent characters"),
        InlineData("old%20guide.md", "old guide.md"),
        InlineData("caf%C3%A9.md", "café.md"),
        InlineData("literal%2520.md", "literal%20.md"),
        InlineData("part%28one%29.md", "part(one).md"),
        InlineData("hash%23name.md", "hash#name.md")]
    public void EncodedPathIsAlreadyCanonical(string destination, string filename)
        => Assert.Empty(Read(destination, Facts(filename)));

    [Theory(DisplayName = "Equivalent path encodings produce an exact Markdown-safe destination"),
        InlineData("%6Fld%20guide.md", "old guide.md", "old%20guide.md"),
        InlineData("caf%c3%a9.md", "café.md", "caf%C3%A9.md"),
        InlineData("./old%20guide.md", "old guide.md", "old%20guide.md")]
    public void NoncanonicalPathUsesEncodedSpelling(string destination, string filename, string intended)
        => Assert.Equal(intended, Assert.Single(Read(destination, Facts(filename))).Intended);

    [Theory(DisplayName = "Readable literal paths are accepted alongside Index's encoded spelling"),
        InlineData("old guide.md", "old guide.md"),
        InlineData("café.md", "café.md"),
        InlineData("part(one).md", "part(one).md"),
        InlineData("café part(one)%23%25%3F.md", "café part(one)#%?.md"),
        InlineData("中文.md", "中文.md")]
    public void LiteralPathIsAccepted(string destination, string filename)
        => Assert.Empty(Read(destination, Facts(filename)));

    [Theory(DisplayName = "Neither unnecessary nor lowercase escaping is an accepted readable spelling"),
        InlineData("%68ash%23name.md", "hash#name.md", "hash%23name.md"),
        InlineData("hash%3fname.md", "hash?name.md", "hash%3Fname.md"),
        InlineData("caf%C3%A9 part(one).md", "café part(one).md", "caf%C3%A9%20part%28one%29.md")]
    public void OtherEncodingsUseIndexSpelling(string destination, string filename, string intended)
        => Assert.Equal(intended, Assert.Single(Read(destination, Facts(filename))).Intended);

    [Fact(DisplayName = "A canonical heading correction keeps the target path percent encoded")]
    public void FragmentCorrectionKeepsEncodedPath()
    {
        var facts = Facts("old guide.md", SourceLinkTargetResolution.FragmentMissing) with { Fragment = "HEADING" };
        facts = facts.WithCanonicalFragment(new("HEADING", "heading"));
        Assert.Equal("old%20guide.md#heading", Assert.Single(Read("old%20guide.md#HEADING", facts)).Intended);
    }

    [Fact(DisplayName = "An unresolved heading never offers an automatic path-only correction")]
    public void MissingFragmentPreventsAutomaticPathCorrection()
        => Assert.Empty(Read("%6Fld%20guide.md#missing", Facts("old guide.md", SourceLinkTargetResolution.FragmentMissing) with { Fragment = "missing" }));

    private static IReadOnlyList<OpenForge.Cli.Core.Framework.Sources.Operational.Models.References.LocalReferenceCanonicalization> Read(
        string destination, SourceLinkDestinationFacts facts)
        => LocalReferenceFactReader.ReadCanonicalizations(".agents/guidance/map.md", destination, new SourceLocation(8, 9, 0, destination.Length), facts);

    private static SourceLinkDestinationFacts Facts(string filename, SourceLinkTargetResolution resolution = SourceLinkTargetResolution.Complete)
        => new()
        {
            Fragment = null,
            Finding = resolution == SourceLinkTargetResolution.FragmentMissing
                ? new() { Code = SourceLinkDestinationFindingCode.FragmentMissing, Cause = "The authored fragment was not found.", Candidates = [] }
                : null,
            Target = new()
            {
                Kind = SourceLinkTargetKind.Local,
                Resolution = resolution,
                Path = $".agents/guidance/{filename}",
                Id = null,
                PhysicalPath = null,
                Layer = null,
                Network = null,
            },
        };
}
