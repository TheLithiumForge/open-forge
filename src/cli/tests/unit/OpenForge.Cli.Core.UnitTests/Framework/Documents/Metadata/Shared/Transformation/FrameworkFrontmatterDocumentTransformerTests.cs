using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Metadata.Shared.Transformation;

public sealed class FrameworkFrontmatterDocumentTransformerTests
{
    [Fact(DisplayName = "Root transformation moves only the leading scoped mapping"), Trait("Feature", "frontmatter-transformation"), Trait("Evidence", "Unit")]
    public void OnlyLeadingMappingMoves()
    {
        const string body = "\n# Body\n\n```yaml\nopen-forge:\n  description: Example\n```\n---\nopen-forge:\n  tags: [Example]\n";
        AssertChanged("---\nopen-forge:\n  description: Purpose\n  tags: [Core]\n---\n" + body,
            "---\ndescription: Purpose\ntags: [Core]\n---\n" + body);
    }

    [Fact(DisplayName = "Root transformation preserves BOM Unicode delimiters and mixed newline bytes"), Trait("Feature", "frontmatter-transformation"), Trait("Evidence", "Unit")]
    public void BomAndMixedNewlinesRemainExact()
        => AssertChanged("\uFEFF--- \t\r\nopen-forge:\n  description: Café 🛠\r  tags: [Core]\r\n---\t \r\n\rBody 🐱\n",
            "\uFEFF--- \t\r\ndescription: Café 🛠\rtags: [Core]\r\n---\t \r\n\rBody 🐱\n");

    [Fact(DisplayName = "Root transformation preserves foreign keys order and comments"), Trait("Feature", "frontmatter-transformation"), Trait("Evidence", "Unit")]
    public void ForeignRootFieldsRemainExact()
        => AssertChanged("---\n# Before\nforeign: {value: é}\nopen-forge: # Container\n  # Purpose\n  description: Purpose # Inline\n  tags: [Core]\n# After\nother:\n  nested: true\n---\nBody\n",
            "---\n# Before\nforeign: {value: é}\n # Container\n# Purpose\ndescription: Purpose # Inline\ntags: [Core]\n# After\nother:\n  nested: true\n---\nBody\n");

    [Theory(DisplayName = "Foreign root ordinary metadata makes flattening invalid")]
    [InlineData("description: Foreign"), InlineData("responsibility: Foreign"), InlineData("tags: [Foreign]")]
    [Trait("Feature", "frontmatter-transformation"), Trait("Evidence", "Unit")]
    public void FlatteningCollisionIsInvalid(string foreign)
        => AssertInvalid($"---\n{foreign}\nopen-forge:\n  description: Purpose\n  tags: [Core]\n---\nBody\n");

    [Theory(DisplayName = "Equivalent root applyTo is retained using the existing normalization")]
    [InlineData("' b, a, b '", "['a', 'b']")]
    [InlineData("'a\\,b'", "['a,b']")]
    [InlineData("['src/**', 'test/**']", "'test/**,src/**'")]
    [Trait("Feature", "frontmatter-transformation"), Trait("Evidence", "Unit")]
    public void EquivalentApplyToIsNotDuplicated(string root, string scoped)
        => AssertChanged($"---\napplyTo: {root}\nopen-forge:\n  description: Purpose\n  applyTo: {scoped}\n  tags: [Core]\n---\nBody\n",
            $"---\napplyTo: {root}\ndescription: Purpose\ntags: [Core]\n---\nBody\n");

    [Fact(DisplayName = "Removing equivalent block applyTo retains every declaration comment"), Trait("Feature", "frontmatter-transformation"), Trait("Evidence", "Unit")]
    public void EquivalentBlockApplyToPreservesComments()
        => AssertChanged("---\napplyTo: 'a,b'\nopen-forge:\n  applyTo: # Declaration\n    - 'b' # Item\n    # Between\n    - 'a'\n  tags: [Core]\n---\nBody\n",
            "---\napplyTo: 'a,b'\n # Declaration\n # Item\n  # Between\ntags: [Core]\n---\nBody\n");

    [Fact(DisplayName = "Removing equivalent inline applyTo retains its trailing comment"), Trait("Feature", "frontmatter-transformation"), Trait("Evidence", "Unit")]
    public void EquivalentInlineApplyToPreservesComment()
        => AssertChanged("---\nopen-forge:\n  description: Purpose\n  applyTo: ['a'] # Scope\n  tags: [Core]\napplyTo: 'a' # Root\n---\n",
            "---\ndescription: Purpose\n # Scope\ntags: [Core]\napplyTo: 'a' # Root\n---\n");

    [Theory(DisplayName = "Conflicting or invalid dual applyTo declarations make flattening invalid")]
    [InlineData("'a'", "'b'")]
    [InlineData("'a,b'", "['a,b']")]
    [InlineData("a", "'a'")]
    [Trait("Feature", "frontmatter-transformation"), Trait("Evidence", "Unit")]
    public void ConflictingApplyToIsInvalid(string root, string scoped)
        => AssertInvalid($"---\napplyTo: {root}\nopen-forge:\n  applyTo: {scoped}\n---\n");

    [Theory(DisplayName = "Native metadata and documents without scoped frontmatter preserve their original bytes")]
    [InlineData("---\nname: native\ndescription: Native purpose\n---\nBody\n")]
    [InlineData("Body\n---\nopen-forge:\n  tags: [Core]\n---\n")]
    [InlineData("---\ndescription: Root purpose\ntags: [Core]\n---\nBody\n")]
    [Trait("Feature", "frontmatter-transformation"), Trait("Evidence", "Unit")]
    public void NativeSkillAndMissingFrontmatterAreUnchanged(string source)
        => AssertUnchanged(Encoding.UTF8.GetBytes(source), FrontmatterForm.Root);

    [Fact(DisplayName = "Scoped rendering restores the identical canonical source memory"), Trait("Feature", "frontmatter-transformation"), Trait("Evidence", "Unit")]
    public void ScopedRenderingRestoresCanonicalSourceBytes()
    {
        ReadOnlyMemory<byte> source = Encoding.UTF8.GetBytes("\uFEFF---\r\nopen-forge:\r\n  tags: [Core]\r\n---\r\nBody\r\n");
        Assert.Equal(FrameworkFrontmatterTransformState.Changed, FrameworkFrontmatterDocumentTransformer.Transform(source, FrontmatterForm.Root).State);
        AssertUnchanged(source, FrontmatterForm.Scoped);
        AssertUnchanged(new byte[] { 0xFF }, FrontmatterForm.Scoped);
    }

    [Theory(DisplayName = "Explicit nonmapping and nonempty flow scoped values cannot be flattened")]
    [InlineData("null"), InlineData("[Core]"), InlineData("*scope"), InlineData("text"), InlineData("{description: Purpose, tags: [Core]}")]
    [Trait("Feature", "frontmatter-transformation"), Trait("Evidence", "Unit")]
    public void NonMappingScopeKeyIsInvalid(string value)
        => AssertInvalid($"---\nopen-forge: {value}\n---\nBody\n");

    [Fact(DisplayName = "Canonical empty scoped mapping is removed without changing foreign fields or body"), Trait("Feature", "frontmatter-transformation"), Trait("Evidence", "Unit")]
    public void EmptyScopeMappingIsRemoved()
        => AssertChanged("---\r\nforeign: true\nopen-forge: {}\r\nother: value\r---\rBody\n",
            "---\r\nforeign: true\nother: value\r---\rBody\n");

    [Fact(DisplayName = "Root transformation preserves multiline scalar and block list values"), Trait("Feature", "frontmatter-transformation"), Trait("Evidence", "Unit")]
    public void MultilineValuesRetainIndentationRelativeToTheirKeys()
        => AssertChanged("---\nopen-forge:\n    description: |\n      First\n      Second\n\n    tags:\n      - Core\n    applyTo:\n      - 'src/**'\n---\nBody\n",
            "---\ndescription: |\n  First\n  Second\n\ntags:\n  - Core\napplyTo:\n  - 'src/**'\n---\nBody\n");

    [Theory(DisplayName = "Malformed or ambiguous leading frontmatter returns a bounded invalid cause")]
    [InlineData("---\nopen-forge:\n  tags: [Core]\n")]
    [InlineData("---\nopen-forge: [\n---\n")]
    [InlineData("---\nopen-forge: {}\nopen-forge: {}\n---\n")]
    [InlineData("---\nopen-forge:\n  {}\n---\n")]
    [Trait("Feature", "frontmatter-transformation"), Trait("Evidence", "Unit")]
    public void InvalidFrontmatterHasNoResultBytes(string source)
        => AssertInvalid(source);

    [Fact(DisplayName = "Root transformation rejects invalid UTF-8 with a bounded cause"), Trait("Feature", "frontmatter-transformation"), Trait("Evidence", "Unit")]
    public void InvalidUtf8HasNoResultBytes()
        => AssertInvalidResult(FrameworkFrontmatterDocumentTransformer.Transform(new byte[] { 0xFF }, FrontmatterForm.Root));

    [Fact(DisplayName = "Frontmatter transformation rejects unnamed form values"), Trait("Feature", "frontmatter-transformation"), Trait("Evidence", "Unit")]
    public void UndefinedFormIsRejected()
        => Assert.Throws<ArgumentOutOfRangeException>(() => FrameworkFrontmatterDocumentTransformer.Transform(ReadOnlyMemory<byte>.Empty, (FrontmatterForm)99));

    private static void AssertChanged(string source, string expected)
    {
        var original = Encoding.UTF8.GetBytes(source);
        var retained = original.ToArray();
        var result = FrameworkFrontmatterDocumentTransformer.Transform(original, FrontmatterForm.Root);
        Assert.Equal(FrameworkFrontmatterTransformState.Changed, result.State);
        Assert.Null(result.Cause);
        Assert.NotNull(result.Bytes);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), result.Bytes.Value.ToArray());
        Assert.Equal(retained, original);
        AssertUnchanged(result.Bytes.Value, FrontmatterForm.Root);
    }

    private static void AssertUnchanged(ReadOnlyMemory<byte> source, FrontmatterForm form)
    {
        var result = FrameworkFrontmatterDocumentTransformer.Transform(source, form);
        Assert.Equal(FrameworkFrontmatterTransformState.Unchanged, result.State);
        Assert.Null(result.Cause);
        Assert.Equal(source, result.Bytes);
    }

    private static void AssertInvalid(string source)
        => AssertInvalidResult(FrameworkFrontmatterDocumentTransformer.Transform(Encoding.UTF8.GetBytes(source), FrontmatterForm.Root));

    private static void AssertInvalidResult(FrameworkFrontmatterTransformResult result)
    {
        Assert.Equal(FrameworkFrontmatterTransformState.Invalid, result.State);
        Assert.Null(result.Bytes);
        Assert.NotNull(result.Cause);
        Assert.InRange(result.Cause.Length, 1, 160);
    }
}
