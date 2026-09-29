using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Metadata.Shared.Applicability;

public sealed class ApplyToMetadataReaderTests
{
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
