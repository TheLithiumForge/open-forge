using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Metadata.Shared.Applicability;

public sealed class ApplyToMetadataReaderTests
{
    [Theory(DisplayName = "Quoted applyTo expressions flatten while list entries stay atomic and trimmed")]
    [InlineData("applyTo: ' src/*.cs, test/*.ts '", new[] { "src/*.cs", "test/*.ts" })]
    [InlineData("open-forge:\n  applyTo: ' src/*.cs, test/*.ts '", new[] { "src/*.cs", "test/*.ts" })]
    [InlineData("applyTo: [' src/*.cs, test/*.ts ']", new[] { "src/*.cs, test/*.ts" })]
    [InlineData("applyTo: 'src/a\\,b.cs,other'", new[] { "other", "src/a,b.cs" })]
    [InlineData("applyTo: ['src/a,b.cs', ' other ']", new[] { "other", "src/a,b.cs" })]
    [InlineData("applyTo: '{src,test}/**/*.{cs,ts},[,]'", new[] { "[,]", "{src,test}/**/*.{cs,ts}" })]
    [Trait("Feature", "apply-to-metadata"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void ReadsExpressionsAndAtomicLists(string yaml, string[] expected)
    {
        var facts = ApplyToMetadataReader.Read(new YamlDocumentParser().Parse(yaml));

        Assert.Equal(ApplyToMetadataState.Valid, facts.State);
        Assert.Equal(expected, facts.Patterns.Select(pattern => pattern.Text));
    }

    [Theory(DisplayName = "Invalid expressions and atomic list entries preserve typed pattern failures")]
    [InlineData("applyTo: ' , , '", nameof(ApplyToPatternFailure.Empty))]
    [InlineData("applyTo: ['   ']", nameof(ApplyToPatternFailure.Empty))]
    [InlineData("applyTo: ['src/a\\,b.cs']", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("applyTo: 'src/a\\\\b.cs'", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [InlineData("applyTo: 'good,../bad'", nameof(ApplyToPatternFailure.Traversal))]
    [InlineData("applyTo: 'good,{bad'", nameof(ApplyToPatternFailure.UnsupportedSyntax))]
    [Trait("Feature", "apply-to-metadata"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void RejectsInvalidPatternValues(string yaml, string expectedFailure)
    {
        var facts = ApplyToMetadataReader.Read(new YamlDocumentParser().Parse(yaml));

        Assert.Equal(ApplyToMetadataState.Invalid, facts.State);
        Assert.Equal(ApplyToMetadataFailureKind.InvalidPattern, facts.Failure?.Kind);
        Assert.Equal(Enum.Parse<ApplyToPatternFailure>(expectedFailure), facts.Failure?.Cause);
        Assert.Empty(facts.Patterns);
        Assert.NotNull(facts.Failure?.Span);
    }

    [Theory(DisplayName = "Root and scoped equivalence compares sets of atomic authored pattern texts")]
    [InlineData("' b, a, b '", "['a', 'b']", true)]
    [InlineData("'a\\,b'", "['a,b']", true)]
    [InlineData("'{a,b}'", "['a', 'b']", false)]
    [InlineData("'a,b'", "['a,b']", false)]
    [InlineData("'{a,b}'", "['{a,b}']", true)]
    [Trait("Feature", "apply-to-metadata"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public void ComparesAtomicSets(string root, string scoped, bool equivalent)
    {
        var document = new YamlDocumentParser().Parse($"applyTo: {root}\nopen-forge:\n  applyTo: {scoped}\n");

        var facts = ApplyToMetadataReader.Read(document);

        Assert.Equal(equivalent ? ApplyToMetadataState.Valid : ApplyToMetadataState.Invalid, facts.State);
        Assert.Equal(2, facts.Declarations.Length);
        if (!equivalent)
        {
            Assert.Equal(ApplyToMetadataFailureKind.ConflictingFields, facts.Failure?.Kind);
        }
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "ApplyTo metadata detects duplicate scoped declarations across duplicate open-forge mappings")]
    [Trait("Feature", "apply-to-metadata"), Trait("Evidence", "Unit")]
    public void DuplicateOpenForgeMappingsCannotHideScopedApplyToDeclarations()
    {
        var document = new YamlDocumentParser().Parse(
            "open-forge:\n  applyTo: '**/*.cs'\nopen-forge:\n  applyTo: '**/*.ts'\n");

        var facts = ApplyToMetadataReader.Read(document);

        Assert.Equal(ApplyToMetadataState.Invalid, facts.State);
        Assert.Equal(ApplyToMetadataFailureKind.DuplicateField, facts.Failure?.Kind);
        Assert.Equal(2, facts.Declarations.Length);
        Assert.All(facts.Declarations, declaration =>
            Assert.Equal(ApplyToMetadataLocation.OpenForge, declaration.Location));
        Assert.Equal(facts.Declarations[1].KeySpan, facts.Failure?.Span);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "ApplyTo metadata ignores duplicate open-forge mappings that declare no applyTo")]
    [Trait("Feature", "apply-to-metadata"), Trait("Evidence", "Unit")]
    public void DuplicateOpenForgeMappingsWithoutApplyToRemainAbsent()
    {
        var document = new YamlDocumentParser().Parse(
            "open-forge:\n  description: First\nopen-forge:\n  description: Second\n");

        var facts = ApplyToMetadataReader.Read(document);

        Assert.Equal(ApplyToMetadataState.Absent, facts.State);
        Assert.Empty(facts.Declarations);
    }
}
