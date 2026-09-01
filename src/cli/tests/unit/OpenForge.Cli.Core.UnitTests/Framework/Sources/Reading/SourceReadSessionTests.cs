using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Reading;

public sealed class SourceReadSessionTests
{
    [Fact(DisplayName = "Neutral source read sessions retain catalogue reader and optional default scope"), Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void SessionRetainsFactsAndRejectsNullRequirements()
    {
        var workspace = Workspace(Path.GetFullPath(Path.Combine(Path.GetTempPath(), "source-read-session-model")));
        var catalogue = new SourceCatalogue(workspace, [], [], [], false);
        var documentReader = new SourceDocumentReader(workspace);
        var scope = new SourceCatalogueSelectionScope(
            SourceLogicalPath.AgentsRoot,
            Path.Combine(workspace.PhysicalRoot, ".agents"));
        var session = new SourceReadSession(catalogue, documentReader, scope);

        Assert.Same(catalogue, session.Catalogue);
        Assert.Same(documentReader, session.DocumentReader);
        Assert.Same(scope, session.DefaultSelectionScope);
        Assert.Throws<ArgumentNullException>(() => new SourceReadSession(null!, documentReader, scope));
        Assert.Throws<ArgumentNullException>(() => new SourceReadSession(catalogue, null!, scope));
    }

    private static CliWorkspace Workspace(string path)
        => new(path, path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
}
