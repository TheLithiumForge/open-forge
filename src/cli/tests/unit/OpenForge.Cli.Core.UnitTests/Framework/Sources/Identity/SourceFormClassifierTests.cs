using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Identity;

public sealed class SourceFormClassifierTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Neutral source-form classification recognizes every accepted source spelling"),
        InlineData(".agents/loader.md", nameof(SourceDocumentForm.Loader)),
        InlineData(".agents/root/_root.md", nameof(SourceDocumentForm.CanonicalEntrypoint)),
        InlineData(".agents/root/index.md", nameof(SourceDocumentForm.IndexEntrypoint)),
        InlineData(".agents/root/_index.md", nameof(SourceDocumentForm.UnderscoreIndexEntrypoint)),
        InlineData(".agents/root/references.md", nameof(SourceDocumentForm.ReferencesEntrypoint)),
        InlineData(".agents/root/_references.md", nameof(SourceDocumentForm.UnderscoreReferencesEntrypoint)),
        InlineData(".agents/skills/example/SKILL.md", nameof(SourceDocumentForm.Skill)),
        InlineData(".agents/root/leaf.md", nameof(SourceDocumentForm.Markdown)),
        InlineData(".agents/root/leaf.overwrite.md", nameof(SourceDocumentForm.OverwriteCompanion))]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void TryClassifyRetainsTheExactIntrinsicForm(
        string canonicalPath,
        string expectedForm)
    {
        var classified = SourceFormClassifier.TryClassify(canonicalPath, out var form);

        Assert.True(classified);
        Assert.Equal(Enum.Parse<SourceDocumentForm>(expectedForm), form);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Neutral source-form matching rejects wrong or noncanonical forms"),
        InlineData(".agents/root/leaf.md", nameof(SourceDocumentForm.Loader)),
        InlineData(".agents/root/leaf.overwrite.md", nameof(SourceDocumentForm.Markdown)),
        InlineData(".agents/root/SKILL.md", nameof(SourceDocumentForm.Markdown)),
        InlineData(".agents/root/../leaf.md", nameof(SourceDocumentForm.Markdown)),
        InlineData(".agents/root\\leaf.md", nameof(SourceDocumentForm.Markdown))]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void MatchesUsesTheCanonicalFormGrammar(
        string canonicalPath,
        string formName)
    {
        Assert.False(SourceFormClassifier.Matches(
            canonicalPath,
            Enum.Parse<SourceDocumentForm>(formName)));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Neutral source-form predicates distinguish canonical and compatibility entrypoints")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void EntrypointPredicatesRetainBothIntrinsicCategories()
    {
        Assert.True(SourceFormClassifier.IsEntrypoint(SourceDocumentForm.CanonicalEntrypoint));
        Assert.True(SourceFormClassifier.IsEntrypoint(SourceDocumentForm.IndexEntrypoint));
        Assert.True(SourceFormClassifier.IsCompatibilityEntrypoint(SourceDocumentForm.IndexEntrypoint));
        Assert.True(SourceFormClassifier.IsCompatibilityEntrypoint(SourceDocumentForm.UnderscoreIndexEntrypoint));
        Assert.True(SourceFormClassifier.IsCompatibilityEntrypoint(SourceDocumentForm.ReferencesEntrypoint));
        Assert.True(SourceFormClassifier.IsCompatibilityEntrypoint(SourceDocumentForm.UnderscoreReferencesEntrypoint));
        Assert.False(SourceFormClassifier.IsCompatibilityEntrypoint(SourceDocumentForm.CanonicalEntrypoint));
        Assert.False(SourceFormClassifier.IsEntrypoint(SourceDocumentForm.Markdown));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Neutral compatibility filename detection is finite and exact"),
        InlineData("index.md", true),
        InlineData("_index.md", true),
        InlineData("references.md", true),
        InlineData("_references.md", true),
        InlineData("_root.md", false),
        InlineData("SKILL.md", false),
        InlineData("index.MD", false)]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void CompatibilityFileNamesHaveNoCaseOrSuffixNormalization(
        string fileName,
        bool expected)
    {
        Assert.Equal(expected, SourceFormClassifier.IsCompatibilityFileName(fileName));
    }
}
