using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational;

public sealed class RouteSourceStructureReaderTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Route source structure keeps absent empty and substantive Axioms distinct")]
    [InlineData("# Scope\n## Entries\n", (int)RouteAxiomsState.Missing)]
    [InlineData("# Scope\n## Axioms\n\n## Entries\n", (int)RouteAxiomsState.Empty)]
    [InlineData("# Scope\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n", (int)RouteAxiomsState.Valid)]
    [InlineData("# Scope\n## Axioms\n\n- A local rule.\n", (int)RouteAxiomsState.Valid)]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void AxiomsStatesRemainDistinct(string source, int expectedState)
    {
        var observation = Read(source);

        Assert.Equal((RouteAxiomsState)expectedState, observation.Axioms.State);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Route source structure keeps duplicate and malformed Axioms headings invalid")]
    [InlineData("# Scope\n## Axioms\n- One.\n## Axioms\n- Two.\n")]
    [InlineData("# Scope\n### Axioms\n- Rule.\n")]
    [InlineData("# Scope\n## axioms\n- Rule.\n")]
    [InlineData("# Scope\n# Axioms\n")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void DuplicateAndMalformedAxiomsHeadingsRemainInvalid(string source)
    {
        var observation = Read(source);

        Assert.Equal(RouteAxiomsState.Invalid, observation.Axioms.State);
        Assert.NotNull(observation.Axioms.Location);
    }

    [Fact(DisplayName = "Doctor reports invalid applyTo when ordinary source metadata is missing"), Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void InvalidApplyToIsReportedIndependentlyOfOrdinaryMetadataState()
    {
        const string source = """
            ---
            applyTo: ["../outside.md"]
            ---
            # Native tool
            """;
        var document = new MarkdownDocumentParser().Parse(source);
        var parsedMetadata = new FrameworkDocumentMetadataParser().Parse(document, FrameworkMetadataReadScope.RoutedSource);
        Assert.Equal(ApplyToMetadataState.Invalid, parsedMetadata.ApplyTo.State);

        var metadataWithoutOrdinaryValues = FrameworkDocumentMetadataFacts.WithoutValues(
            FrameworkDocumentMetadataState.Missing) with
        {
            ApplyTo = parsedMetadata.ApplyTo,
        };
        Assert.Equal(FrameworkDocumentMetadataState.Missing, metadataWithoutOrdinaryValues.State);

        var issues = RouteSourceStructureReader.ReadWorkspaceIssues(
            ".agents/skills/tool/SKILL.md",
            document,
            metadataWithoutOrdinaryValues,
            new Utf8SourceMap(source));

        var issue = Assert.Single(issues);
        Assert.Equal(RouteWorkspaceSourceIssueKind.FrontmatterMalformed, issue.Kind);
        Assert.NotNull(issue.Location);
    }

    [Fact(DisplayName = "Doctor accepts equivalent dual applyTo declarations for future files"), Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void EquivalentDualApplyToDeclarationsDoNotCreateWorkspaceIssues()
    {
        const string source = """
            ---
            applyTo: ["future/**/*.cs"]
            open-forge:
              description: Future guidance
              tags: [Guidance]
              applyTo: ["future/**/*.cs"]
            ---
            # Future guidance
            """;
        var document = new MarkdownDocumentParser().Parse(source);
        var metadata = new FrameworkDocumentMetadataParser().Parse(document, FrameworkMetadataReadScope.RoutedSource);

        Assert.Equal(ApplyToMetadataState.Valid, metadata.ApplyTo.State);
        Assert.Empty(RouteSourceStructureReader.ReadWorkspaceIssues(
            ".agents/guidance/future.md",
            document,
            metadata,
            new Utf8SourceMap(source)));
    }

    private static RouteSourceStructureObservation Read(string source)
    {
        var document = new MarkdownDocumentParser().Parse(source);
        return RouteSourceStructureReader.ReadStructure(
            document,
            SourceDocumentForm.CanonicalEntrypoint,
            new Utf8SourceMap(source));
    }
}
