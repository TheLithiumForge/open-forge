using OpenForge.Cli.Core.Commands.References.Models.Resolution;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Shared.Parsing;
using OpenForge.Cli.Core.Commands.References.Shared.Resolution;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.References;

[Trait("Feature", "references"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ReferencesReadSessionIntegrationTests
{
    [Fact(DisplayName = "Repeated link targets resolve and read freshly while identical content parses once")]
    public async Task RepeatedTargetsReuseAcquisition()
    {
        using var temporary = TemporaryWorkspace.Create("references-read-session");
        temporary.CreateFile(".agents/docs/source.md", "[First](target.md#first) [Second](target.md#second)\n");
        temporary.CreateFile(".agents/docs/target.md", "# First\n\n## Second\n");
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var token = TestContext.Current.CancellationToken;
        var catalogue = await new SourceCatalogueReader().ReadAsync(new SourceCatalogueRequest(workspace, [".agents"]), token);
        var source = catalogue.FindByPath(".agents/docs/source.md") ?? throw new InvalidOperationException();
        var resolutions = 0;
        var reads = 0;
        var parses = 0;
        var resolver = new ReferencesDestinationResolver((selected, path) =>
        {
            resolutions++;
            return new PhysicalPathResolver().ResolveCandidate(selected.LexicalRoot, selected.PhysicalRoot, path);
        }, (physical, logical, cancellation) =>
        {
            reads++;
            return StrictUtf8FileReader.ReadAsync(physical, logical, cancellation);
        }, text =>
        {
            parses++;
            return new MarkdownDocumentParser().Parse(text);
        });
        var session = resolver.CreateReadSession(new MarkdownDocumentCache());
        for (var index = 0; index < 50; index++)
        {
            var fragment = index % 2 == 0 ? "first" : "second";
            var result = await session.ResolveAsync(new ReferencesDestinationInput(workspace, catalogue, source, source.Base, $"target.md#{fragment}"), token);
            Assert.Null(result.Finding);
        }
        Assert.Equal(50, resolutions);
        Assert.Equal(50, reads);
        Assert.Equal(1, parses);

        temporary.ReplaceText(".agents/docs/target.md", "# Changed\n");
        var refreshed = await session.ResolveAsync(new ReferencesDestinationInput(workspace, catalogue, source, source.Base, "target.md#first"), token);
        Assert.NotNull(refreshed.Finding);
        Assert.Equal(51, resolutions);
        Assert.Equal(51, reads);
        Assert.Equal(2, parses);
    }
}
