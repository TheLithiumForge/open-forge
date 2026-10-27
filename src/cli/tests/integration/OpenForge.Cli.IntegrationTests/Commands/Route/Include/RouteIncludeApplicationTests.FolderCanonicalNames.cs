using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Include;

public sealed partial class RouteIncludeApplicationTests
{
    [Theory(DisplayName = "Folder Include accepts canonical anchors whose names also have compatibility meaning"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(false, "index", false), InlineData(false, "index", true)]
    [InlineData(true, "index", false), InlineData(true, "index", true)]
    [InlineData(false, "references", false), InlineData(false, "references", true)]
    [InlineData(true, "references", false), InlineData(true, "references", true)]
    public async Task CanonicalCompatibilityNameApplyAndNoOp(bool custom, string name, bool anchor)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom);
        var folder = "docs/" + name;
        var catalogue = WorkspaceRoutePath.CanonicalEntrypointPath(folder);
        workspace.Files.CreateDirectory(folder);
        if (anchor) workspace.Files.WriteText(catalogue, RouteInclusionFolderFixture.AuthoredCatalogue);
        else workspace.ExpectCreate(catalogue);
        var before = anchor ? workspace.Bytes(catalogue) : null;
        var operation = new RouteIncludeOperation(workspace.Locks.StoreRoot);
        var request = new RouteIncludeRequest { Workspace = workspace.Workspace, File = folder };

        var result = await operation.ExecuteAsync(request, RouteInclusionFolderFixture.Token);

        Assert.True(result.Status == CliSemanticStatus.Complete,
            $"Status={result.Status}, code={result.Outcome.Code}, verified={result.Outcome.Verified}, anchor={anchor}");
        Assert.True(result.Outcome.Verified);
        Assert.False(result.Outcome.NoOp);
        Assert.All(result.Outcome.Effects, effect => Assert.Equal(RouteInclusionEffectState.Verified, effect.State));
        Assert.Equal(anchor ? 2 : 3, result.Outcome.Effects.Length);
        Assert.Equal(anchor ? 0 : 1, result.Outcome.Effects.Count(effect => effect.Kind == RouteInclusionEffectKind.Catalogue));
        var settings = RouteInclusionFolderFixture.Settings(workspace);
        Assert.Equal([folder], settings.OutsideRoutes);
        Assert.Empty(settings.Roots);
        var layout = WorkspaceLayout.Create(workspace.Workspace, settings.MainRoot).WithOutsideRoutes(settings.OutsideRoutes);
        Assert.Equal(catalogue, Assert.Single(SourceLoaderEntriesParser.Parse(layout, workspace.Read(layout.LoaderPath)).Destinations).CanonicalPath);
        var after = workspace.Bytes(catalogue);
        if (before is not null) Assert.Equal(before, after);
        var hashes = workspace.Files.SnapshotHashes();

        var repeated = await operation.ExecuteAsync(request, RouteInclusionFolderFixture.Token);

        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.True(repeated.Outcome.Verified);
        Assert.True(repeated.Outcome.NoOp);
        Assert.Empty(repeated.Outcome.Effects);
        Assert.Equal(after, workspace.Bytes(catalogue));
        Assert.Equal(hashes, workspace.Files.SnapshotHashes());
    }
}
