using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Sources.Shared.Destinations;
using OpenForge.Cli.Core.UnitTests.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Shared.Destinations;

[Trait("Feature", "reference-encoding"), Trait("Evidence", "Unit")]
public sealed class SourceDestinationSpellingTests
{
    [Theory(DisplayName = "Both accepted destination spellings preserve entity-bearing filename identity through Markdown and resolution"),
        InlineData("entity&amp;.md", "entity%26amp;.md"),
        InlineData("entity&AMP;.md", "entity%26AMP;.md"),
        InlineData("entity&apos;.md", "entity%26apos;.md"),
        InlineData("entity&sol;.md", "entity%26sol;.md"),
        InlineData("entity&bsol;.md", "entity%26bsol;.md"),
        InlineData("entity&NotEqualTilde;.md", "entity%26NotEqualTilde;.md"),
        InlineData("entity&#65;.md", "entity&%2365;.md"),
        InlineData("entity&#x41;.md", "entity&%23x41;.md"),
        InlineData("entity&#X41;.md", "entity&%23X41;.md"),
        InlineData("entity&#0;.md", "entity&%230;.md"),
        InlineData("entity&amp;&amp;.md", "entity%26amp;%26amp;.md"),
        InlineData("entity&amp;amp;.md", "entity%26amp;amp;.md")]
    public async Task EntitySpellingsPreserveIdentity(string filename, string literal)
    {
        await AssertBothSpellingsResolveAsync(filename);
        Assert.Equal(literal, SourceDestinationEncoder.EncodeLiteralPath(filename));
    }

    [Theory(DisplayName = "Ordinary ampersands and unrecognized or incomplete entities remain readable"),
        InlineData("ordinary&name.md"), InlineData("ordinary&unknownentity;.md"),
        InlineData("ordinary&amp.md"), InlineData("ordinary&&.md")]
    public async Task OrdinaryAmpersandsRemainLiteral(string filename)
    {
        Assert.Equal(filename, SourceDestinationEncoder.EncodeLiteralPath(filename));
        await AssertBothSpellingsResolveAsync(filename);
    }

    [Fact(DisplayName = "Every supported ASCII punctuation character preserves the literal destination target")]
    public async Task AllSupportedAsciiPunctuationPreservesIdentity()
    {
        foreach (var character in Enumerable.Range('!', '~' - '!' + 1).Select(value => (char)value)
                     .Where(character => !char.IsAsciiLetterOrDigit(character) && character is not ('/' or '\\')))
            await AssertBothSpellingsResolveAsync($"punctuation{character}name.md");
    }

    [Theory(DisplayName = "Angle delimiters and literal Unicode preserve destination identity without normalization"),
        InlineData("space name.md"), InlineData("unbalanced(one.md"), InlineData("café中文😀.md"),
        InlineData("combining-e\u0301.md"), InlineData("nonbreaking\u00a0space.md")]
    public Task LiteralUnicodeAndDelimitersPreserveIdentity(string filename)
        => AssertBothSpellingsResolveAsync(filename);

    private static async Task AssertBothSpellingsResolveAsync(string filename)
    {
        var workspace = SourceInventoryTestData.Workspace();
        var catalogue = new SourceCatalogue(WorkspaceLayout.Create(workspace, WorkspaceLayout.DefaultMainRoot), [], [], [], isCancelled: false);
        // Resolve the logical identity before I/O, including names forbidden by this host's filesystem.
        var resolver = new SourceLinkDestinationResolver(
            physicalPathResolver: (_, path) => PhysicalPathResolution.Classified(PhysicalPathState.Missing, path),
            strictUtf8Reader: (_, _, _) => throw new InvalidOperationException("No target bytes are needed."),
            markdownParser: _ => throw new InvalidOperationException("No target headings are needed."));
        foreach (var destination in new[] { SourceDestinationEncoder.EncodePath(filename), SourceDestinationEncoder.EncodeLiteralPath(filename) })
        {
            var link = Assert.Single(new MarkdownDocumentParser().Parse($"[Guide](<{destination}>)\n").Links);
            var facts = await resolver.ResolveAsync(new SourceLinkDestinationInput
            {
                Workspace = workspace,
                Catalogue = catalogue,
                SourceCanonicalPath = ".agents/guidance/map.md",
                RawDestination = link.RawDestination,
            }, TestContext.Current.CancellationToken);
            Assert.Equal($".agents/guidance/{filename}", facts.Target.Path);
            Assert.Equal(SourceLinkTargetResolution.Missing, facts.Target.Resolution);
            Assert.Null(facts.Fragment);
            Assert.Equal(destination, link.RawDestination);
        }
    }
}
