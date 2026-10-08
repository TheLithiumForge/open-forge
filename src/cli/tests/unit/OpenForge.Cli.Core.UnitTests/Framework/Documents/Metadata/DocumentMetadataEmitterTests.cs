using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Metadata;

public sealed class DocumentMetadataEmitterTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Framework metadata emits applyTo as a quoted scoped list and reads it back")]
    [Trait("Feature", "apply-to-metadata"), Trait("Evidence", "Unit")]
    public void EmitsAndReadsScopedApplyToList()
    {
        var patterns = new[]
        {
            new ApplyToPattern("**/*.cs", ["**", "*.cs"]),
            new ApplyToPattern("src/*.cs", ["src", "*.cs"]),
        };
        var yaml = new FrameworkDocumentMetadataEmitter().EmitOptional(
            new FrameworkDocumentMetadataEmission("Route description", ["Docs"], null, patterns), FrontmatterForm.Scoped);

        Assert.Contains("applyTo: [\"**/*.cs\", \"src/*.cs\"]", yaml, StringComparison.Ordinal);
        var facts = new FrameworkDocumentMetadataParser().Parse(
            new MarkdownDocumentParser().Parse($"---\n{yaml}---\n"), FrameworkMetadataReadScope.RoutedSource);
        Assert.Equal(ApplyToMetadataState.Valid, facts.ApplyTo.State);
        Assert.Equal(patterns.Select(pattern => pattern.Text), facts.ApplyTo.Patterns.Select(pattern => pattern.Text));
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Framework metadata round trips authored glob syntax through a quoted scoped list")]
    [InlineData("{src,test}/**/*.{cs,ts}")]
    [InlineData("src/[ab].cs")]
    [InlineData("src/report,legacy.cs")]
    [Trait("Feature", "apply-to-metadata"), Trait("Evidence", "Unit")]
    public void EmitsAndReadsAuthoredApplyToPatterns(string text)
    {
        var pattern = new ApplyToPattern(text, [.. text.Split('/')]);
        var yaml = new FrameworkDocumentMetadataEmitter().EmitOptional(
            new FrameworkDocumentMetadataEmission("Route description", ["Docs"], null, [pattern]), FrontmatterForm.Scoped);

        Assert.Contains($"applyTo: [\"{text}\"]", yaml, StringComparison.Ordinal);
        var facts = new FrameworkDocumentMetadataParser().Parse(
            new MarkdownDocumentParser().Parse($"---\n{yaml}---\n"), FrameworkMetadataReadScope.RoutedSource);

        Assert.Equal(ApplyToMetadataState.Valid, facts.ApplyTo.State);
        Assert.Equal([text], facts.ApplyTo.Patterns.Select(pattern => pattern.Text));
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Optional metadata omits missing fields while retaining safe authored values")]
    [InlineData(null, false)]
    [InlineData("true: # 路由", false)]
    [InlineData(null, true)]
    [Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void OptionalFieldsRemainMissing(string? description, bool hasTags)
    {
        string[] tags = hasTags ? ["Évidence2", "工作-2"] : [];
        var yaml = new FrameworkDocumentMetadataEmitter().EmitOptional(
            new FrameworkDocumentMetadataEmission(description, tags, null), FrontmatterForm.Scoped);
        var document = new MarkdownDocumentParser().Parse($"---\n{yaml}---\n# Body\n");
        var facts = new FrameworkDocumentMetadataParser().Parse(document, FrameworkMetadataReadScope.RoutedSource);

        Assert.Equal(FrameworkDocumentMetadataState.Missing, facts.State);
        Assert.Equal(description, facts.ObservedDescription);
        Assert.Equal(tags, facts.ObservedTags);
        Assert.Contains("open-forge:", yaml, StringComparison.Ordinal);
        Assert.DoesNotContain("responsibility:", yaml, StringComparison.Ordinal);
        Assert.DoesNotContain("\r", yaml, StringComparison.Ordinal);
        if (description is null)
        {
            Assert.DoesNotContain("description:", yaml, StringComparison.Ordinal);
        }
        if (!hasTags)
        {
            Assert.DoesNotContain("tags:", yaml, StringComparison.Ordinal);
        }
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Optional emission rejects malformed present fields and owns ordered tags")]
    [Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void OptionalModelRetainsValidationAndImmutableValues()
    {
        Assert.Throws<ArgumentException>(() => new FrameworkDocumentMetadataEmission(" ", [], null));
        Assert.Throws<ArgumentException>(() => new FrameworkDocumentMetadataEmission(null, ["bad tag"], null));
        Assert.Throws<ArgumentException>(() => new FrameworkDocumentMetadataEmission(null, ["Good", "Good"], null));
        Assert.Throws<ArgumentException>(() => new FrameworkDocumentMetadataEmission(null, [null!], null));
        Assert.Throws<ArgumentNullException>(() => new FrameworkDocumentMetadataEmission(null, null!, null));
        Assert.Throws<ArgumentException>(() => new FrameworkDocumentMetadataEmission(null, [], ""));
        var tags = new[] { "Second", "First" };
        var metadata = new FrameworkDocumentMetadataEmission(null, tags, null);
        tags[0] = "Changed";
        Assert.Equal(["Second", "First"], metadata.Tags);
    }

    [Theory(DisplayName = "Canonical metadata forms round trip every supported field through one grammar")]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Boundary", "Output"), Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void CanonicalFormsRoundTripAllSupportedFields(bool root)
    {
        var form = root ? FrontmatterForm.Root : FrontmatterForm.Scoped;
        var metadata = new FrameworkDocumentMetadata(
            "true: # 路由",
            ["Évidence2", "工作-2"],
            "null: # ownership",
            [new ApplyToPattern("**/*.cs", ["**", "*.cs"])]);
        var yaml = new FrameworkDocumentMetadataEmitter().Emit(metadata, form);
        var facts = new FrameworkDocumentMetadataParser().Parse(
            new MarkdownDocumentParser().Parse($"---\n{yaml}---\n"), FrameworkMetadataReadScope.RoutedSource);

        Assert.Equal(FrameworkDocumentMetadataState.Complete, facts.State);
        var actual = Assert.IsType<FrameworkDocumentMetadata>(facts.Metadata);
        Assert.Equal(metadata.Description, actual.Description);
        Assert.Equal(metadata.Tags, actual.Tags);
        Assert.Equal(metadata.Responsibility, actual.Responsibility);
        Assert.Equal(metadata.ApplyTo.Select(pattern => pattern.Text), actual.ApplyTo.Select(pattern => pattern.Text));
        Assert.Equal(Assert.Single(metadata.ApplyTo).Segments, Assert.Single(actual.ApplyTo).Segments);
        Assert.Equal(form, facts.Syntax.AuthoredForm);
        Assert.DoesNotContain("\r", yaml, StringComparison.Ordinal);
        Assert.EndsWith("\n", yaml, StringComparison.Ordinal);
        Assert.Equal(root, !yaml.StartsWith("open-forge:", StringComparison.Ordinal));
    }

    [Theory(DisplayName = "Optional metadata forms omit missing values and retain incomplete observations")]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [Trait("Boundary", "Output"), Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void OptionalFormsDoNotInventMissingValues(bool root, bool hasDescription, bool hasTags)
    {
        var form = root ? FrontmatterForm.Root : FrontmatterForm.Scoped;
        var description = hasDescription ? "Description" : null;
        string[] tags = hasTags ? ["Docs"] : [];
        var yaml = new FrameworkDocumentMetadataEmitter().EmitOptional(new FrameworkDocumentMetadataEmission(description, tags, null), form);
        var facts = new FrameworkDocumentMetadataParser().Parse(
            new MarkdownDocumentParser().Parse($"---\n{yaml}---\n"), FrameworkMetadataReadScope.RoutedSource);

        Assert.Equal(FrameworkDocumentMetadataState.Missing, facts.State);
        Assert.Equal(description, facts.ObservedDescription);
        Assert.Equal(tags, facts.ObservedTags);
        Assert.Equal(hasDescription, yaml.Contains("description:", StringComparison.Ordinal));
        Assert.Equal(hasTags, yaml.Contains("tags:", StringComparison.Ordinal));
        Assert.DoesNotContain("responsibility:", yaml, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Root metadata emits the same ordered canonical fields without the scoped container")]
    [Trait("Boundary", "Output"), Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void RootOutputRetainsCanonicalFieldOrderAndQuoting()
    {
        var metadata = new FrameworkDocumentMetadata("Description", ["Docs"], "Owner", [new ApplyToPattern("**/*.cs", ["**", "*.cs"])]);

        var yaml = new FrameworkDocumentMetadataEmitter().Emit(metadata, FrontmatterForm.Root);

        Assert.Equal("description: Description\ntags: [Docs]\nresponsibility: Owner\napplyTo: [\"**/*.cs\"]\n", yaml);
    }

    [Fact(DisplayName = "Required metadata emission rejects undefined frontmatter forms")]
    [Trait("Boundary", "Output"), Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void UndefinedFormIsRejected()
    {
        var emitter = new FrameworkDocumentMetadataEmitter();
        var form = (FrontmatterForm)int.MaxValue;

        Assert.Throws<ArgumentOutOfRangeException>(() => emitter.Emit(new FrameworkDocumentMetadata("Description", ["Docs"], null), form));
        Assert.Throws<ArgumentOutOfRangeException>(() => emitter.EmitOptional(new FrameworkDocumentMetadataEmission(null, [], null), form));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Framework metadata emits one ordered Open Forge root without absent values")]
    [Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void EmitsCanonicalOpenForgeDocumentWithoutResponsibility()
    {
        var yaml = new FrameworkDocumentMetadataEmitter().Emit(
            new FrameworkDocumentMetadata("Route description", ["NeedsAuthoring", "工作2"], null), FrontmatterForm.Scoped);

        Assert.Equal(
            "open-forge:\n"
                + "  description: Route description\n"
                + "  tags: [NeedsAuthoring, 工作2]\n",
            yaml);
        Assert.DoesNotContain("rune", yaml, StringComparison.Ordinal);
        Assert.DoesNotContain("responsibility", yaml, StringComparison.Ordinal);
        Assert.DoesNotContain("\r", yaml, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Framework metadata emits responsibility after ordered tags")]
    [Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void EmitsCanonicalOpenForgeDocumentWithResponsibility()
    {
        var yaml = new FrameworkDocumentMetadataEmitter().Emit(
            new FrameworkDocumentMetadata(
                "Route description",
                ["NeedsAuthoring"],
                "Owns route documentation."), FrontmatterForm.Scoped);

        Assert.Equal(
            "open-forge:\n"
                + "  description: Route description\n"
                + "  tags: [NeedsAuthoring]\n"
                + "  responsibility: Owns route documentation.\n",
            yaml);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Framework metadata round trips Unicode and YAML-sensitive scalars")]
    [Trait("Feature", "framework-document-metadata"), Trait("Evidence", "Unit")]
    public void EmittedYamlRoundTripsThroughCanonicalParser()
    {
        var expected = new FrameworkDocumentMetadata(
            "true: # 路由",
            ["Évidence2", "工作-2"],
            "null: # ownership");
        var yaml = new FrameworkDocumentMetadataEmitter().Emit(expected, FrontmatterForm.Scoped);
        var document = new MarkdownDocumentParser().Parse($"---\n{yaml}---\n# Body\n");

        var facts = new FrameworkDocumentMetadataParser().Parse(document, FrameworkMetadataReadScope.RoutedSource);

        Assert.Equal(FrameworkDocumentMetadataState.Complete, facts.State);
        var actual = Assert.IsType<FrameworkDocumentMetadata>(facts.Metadata);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.Tags, actual.Tags);
        Assert.Equal(expected.Responsibility, actual.Responsibility);
    }
}
