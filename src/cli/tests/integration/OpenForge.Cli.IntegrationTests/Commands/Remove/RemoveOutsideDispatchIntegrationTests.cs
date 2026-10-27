using OpenForge.Cli.Core.Framework.Sources.Identity;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Remove;
using OpenForge.Cli.Core.Commands.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Remove.Models.Selection;
using OpenForge.Cli.Core.Commands.Remove.Shared.Selection;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.IntegrationTests.Commands.Route.Remove;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Route.Remove.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.IntegrationTests.Commands.Remove;

[Trait("Boundary", "Host"), Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
public sealed class RemoveOutsideDispatchIntegrationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task RootDispatchRemovesOneBoundedRoute(bool category, bool custom)
    {
        using var fixture = new RouteRemoveOutsideWorkspace(custom ? RouteRemoveOutsideWorkspace.CustomRoot : WorkspaceLayout.DefaultMainRoot, listed: false, compatibility: true);
        if (category) fixture.SeedCategory();
        var path = category ? RouteRemoveOutsideWorkspace.Category : RouteRemoveOutsideWorkspace.Overwrite;
        var loader = fixture.Inner.ReadBytes(fixture.Layout.LoaderPath);
        using var output = new StringWriter();
        using var error = new StringWriter();
        var result = await fixture.Inner.RunAsync([RemoveDefinitions.CommandIdentity, SourceWorkspaceRelativePath.LiteralReferencePrefix + path, RemoveDefinitions.AutomaticOption,
            CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat], output, error);
        Assert.Equal(0, result.ExitCode);
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Equal(RemoveDefinitions.CommandIdentity, report.RootElement.GetProperty(CliJsonFields.Command).GetString());
        Assert.All(report.RootElement.GetProperty(CliJsonFields.Data).GetProperty(JsonNamingPolicy.CamelCase.ConvertName(nameof(RouteRemoveData.Removed))).EnumerateArray(),
            removed => Assert.StartsWith(RouteRemoveOutsideWorkspace.DirectoryPath + "/", removed.GetString(), StringComparison.Ordinal));
        Assert.Equal(loader, fixture.Inner.ReadBytes(fixture.Layout.LoaderPath));
        Assert.Empty(fixture.Inner.ReadSettings().RemovedCategories);
        Assert.False(category ? Directory.Exists(fixture.Inner.Combine(path)) : File.Exists(fixture.Inner.Combine(RouteRemoveOutsideWorkspace.Leaf)));
    }

    [Theory]
    [InlineData("docs")]
    [InlineData(RouteRemoveOutsideWorkspace.DirectoryPath)]
    [InlineData(RouteRemoveOutsideWorkspace.DirectoryPath + "/_team.md")]
    [InlineData(RouteRemoveOutsideWorkspace.DirectoryPath + "/index.md")]
    [InlineData(RouteRemoveOutsideWorkspace.DirectoryPath + "/_index.overwrite.md")]
    public async Task RootProtectionDoesNotRequirePresentAnchor(string path)
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        fixture.Inner.DeleteFile(fixture.Anchor);
        var before = fixture.Inner.SnapshotHashes();
        using var output = new StringWriter();
        using var error = new StringWriter();
        var result = await fixture.Inner.RunAsync([RemoveDefinitions.CommandIdentity, SourceWorkspaceRelativePath.LiteralReferencePrefix + path, RemoveDefinitions.AutomaticOption,
            CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat], output, error);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
        fixture.Inner.AssertNoLockInfrastructure();
    }

    [Fact]
    public async Task DescendantEntrypointFileRetainsCategorySelectionRefusal()
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        fixture.SeedCategory();
        var path = WorkspaceRoutePath.CanonicalEntrypointPath(RouteRemoveOutsideWorkspace.Category);
        var selected = await RemoveTargetResolver.ResolveAsync(fixture.Inner.Workspace,
            RemoveSelection.Selected(RemoveKind.Path, path), TestContext.Current.CancellationToken);
        Assert.True(selected.IsEntrypointFile);
        var before = fixture.Inner.SnapshotHashes();
        using var output = new StringWriter();
        using var error = new StringWriter();
        var result = await fixture.Inner.RunAsync([RemoveDefinitions.CommandIdentity, SourceWorkspaceRelativePath.LiteralReferencePrefix + path, RemoveDefinitions.AutomaticOption,
            CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat], output, error);
        Assert.NotEqual(0, result.ExitCode);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
    }

    [Theory]
    [InlineData("file")]
    [InlineData("category")]
    [InlineData("overwrite")]
    [InlineData("missing-directory")]
    public async Task FinalPathAuthorityRefusesAfterRegistrationDrift(string kind)
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        fixture.Inner.WriteText(WorkspaceSettingsDefinitions.RelativePath, fixture.Settings(routes: []));
        var path = kind switch
        {
            "file" => RouteRemoveOutsideWorkspace.Leaf,
            "overwrite" => RouteRemoveOutsideWorkspace.Overwrite,
            _ => RouteRemoveOutsideWorkspace.Category,
        };
        if (kind == "category") fixture.SeedCategory();
        var selection = RemoveSelection.Selected(RemoveKind.Path, path);
        var dispatch = await RemoveTargetResolver.ResolveAsync(fixture.Inner.Workspace, selection, TestContext.Current.CancellationToken);
        Assert.Equal(RemoveKind.Path, dispatch.Kind);
        fixture.Inner.WriteText(WorkspaceSettingsDefinitions.RelativePath, fixture.Settings());
        var before = fixture.Inner.SnapshotHashes();
        var result = await RemovePathOperationFactory.Create(fixture.Inner.LockStoreRoot).ExecuteAsync(
            new RemovePathRequest(fixture.Inner.Workspace, path, RemoveMode.Apply, automatic: true), TestContext.Current.CancellationToken);
        Assert.NotEqual(CliSemanticStatus.Complete, result.Status);
        Assert.NotEmpty(result.Findings);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
        fixture.Inner.AssertNoLockInfrastructure();
    }
}
