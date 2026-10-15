using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.IntegrationTests.Framework.Sources.Shared;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Operational;

public sealed class LocalReferenceInventoryWorkTests
{
    [Fact(DisplayName = "Reference inventory shares the diagnosed catalogue and documents without another source parse"),
        Trait("Feature", "references"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task DiagnosisRetainsOneCatalogueAndParse()
    {
        using var workspace = SourceIntegrationWorkspace.Create("reference-inventory-work");
        workspace.Write("AGENTS.md", "# Workspace\n");
        workspace.Write(".agents/loader.md", "# Loader\n");
        workspace.Write(".agents/source.md", "# Source\n\n[Target](target.md#target)\n");
        workspace.Write(".agents/target.md", "# Target\n");
        var paths = new PhysicalPathResolver();
        var inspector = new RouteSourceInspector(new SourceReadSessionReader(paths), paths);
        var routes = await new RouteObservationReader(inspector, new RouteGeneratedNavigationReader(paths), new RouteDoctorGeneratedNavigationReader())
            .ReadDoctorAsync(workspace.Workspace, sharing: null, TestContext.Current.CancellationToken);
        var parses = 0;
        var reads = 0;
        var acquisition = Assert.IsType<OpenForge.Cli.Core.Framework.Sources.Models.Reading.SourceReadSession>(routes.Session).DocumentReader.Acquisition;
        var acquiredBefore = acquisition.ReadCount;
        var contributor = new LocalReferenceOperationalContributor(inspector, new SourceLinkDestinationResolver(
            (selected, path) => paths.ResolveCandidate(selected.LexicalRoot, selected.PhysicalRoot, path),
            (path, logical, token) => { reads++; return StrictUtf8FileReader.ReadAsync(path, logical, token); },
            text => { parses++; return new MarkdownDocumentParser().Parse(text); }), new LocalReferenceCandidateReader());

        var references = await contributor.ReadDoctorAsync(workspace.Workspace, TestContext.Current.CancellationToken, routes);

        Assert.Equal(0, parses);
        Assert.Equal(0, reads);
        Assert.Equal(acquiredBefore + 1, acquisition.ReadCount);
        Assert.Same(routes.Catalogue, references.Catalogue);
        foreach (var source in routes.Sources)
            Assert.Same(source.Document, references.Documents[source.Source.Identity.CanonicalBasePath]);
        Assert.Single(references.References);
    }
}
