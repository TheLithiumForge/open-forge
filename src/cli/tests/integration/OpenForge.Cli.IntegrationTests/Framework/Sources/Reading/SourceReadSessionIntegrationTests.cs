using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Reading;

public sealed class SourceReadSessionIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Neutral source read reader establishes the agents catalogue document reader and contained default scope"), Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task ReaderEstablishesContainedAgentsSession()
    {
        using var temporary = TemporaryWorkspace.Create("source-read-session-contained");
        temporary.CreateDirectory(".agents");
        temporary.CreateFile(".agents/loader.md", "# Loader");
        var workspace = Workspace(temporary.Path);

        var session = await new SourceReadSessionReader(new PhysicalPathResolver())
            .ReadAsync(workspace, TestContext.Current.CancellationToken);

        Assert.Same(workspace, session.Catalogue.Workspace);
        Assert.Same(workspace, session.DocumentReader.Workspace);
        var scope = Assert.IsType<SourceCatalogueSelectionScope>(session.DefaultSelectionScope);
        Assert.Equal(SourceLogicalPath.AgentsRoot, scope.CanonicalDirectoryPath);
        Assert.Equal(temporary.Combine(".agents"), scope.PhysicalDirectoryPath);
        Assert.NotNull(session.Catalogue.FindByPath(".agents/loader.md"));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Neutral source read reader omits the default scope when the agents root is not contained"), Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task ReaderOmitsScopeForMissingAgentsRoot()
    {
        using var temporary = TemporaryWorkspace.Create("source-read-session-missing");
        var workspace = Workspace(temporary.Path);

        var session = await new SourceReadSessionReader(new PhysicalPathResolver())
            .ReadAsync(workspace, TestContext.Current.CancellationToken);

        Assert.Null(session.DefaultSelectionScope);
        Assert.Contains(
            session.Catalogue.SelectAll().RootIssues,
            issue => issue.Code == SourceCatalogueIssueCode.RootMissing);
    }

    private static CliWorkspace Workspace(string path)
        => new(path, path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
}
