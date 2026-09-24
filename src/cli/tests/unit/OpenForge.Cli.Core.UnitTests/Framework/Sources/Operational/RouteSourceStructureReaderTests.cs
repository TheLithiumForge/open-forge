using OpenForge.Cli.Core.Framework.Documents.Markdown;
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

    private static RouteSourceStructureObservation Read(string source)
    {
        var document = new MarkdownDocumentParser().Parse(source);
        return RouteSourceStructureReader.ReadStructure(
            document,
            SourceDocumentForm.CanonicalEntrypoint,
            new Utf8SourceMap(source));
    }
}
